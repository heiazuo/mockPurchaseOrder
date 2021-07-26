using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using MopckPurchaseOrder.Log;
using MopckPurchaseOrder.setting;
using Newtonsoft.Json;
using Date = MopckPurchaseOrder.model.Date;

namespace MopckPurchaseOrder
{
    public class Statistics
    {
        public int orderCount = 0;
        public decimal orderAmount = 0;
    }

    static class Program
    {

        private static readonly string PathUrl = Environment.CurrentDirectory + "\\setting.json";
        private static readonly int DataFactoryTaskNum = 100; //线程任务数
        private static Statistics statistics = new Statistics();

        static void Main(string[] args)
        {
            var setting = JsonConvert.DeserializeObject<setting.setting>(File.ReadAllText(PathUrl));
            if (setting == null)
            {
                Console.WriteLine("没有配置文件");
                return;
            }

            var dbContext = new ErpDataBase();
            var userList = dbContext.View_Sys_Users.Where(p => p.BranchId == setting.BranchId).Where(p => p.DeptName.Contains("大客户销售事业部") || p.DeptName.Contains("总经办")).Select(p => p.Id).ToList();
            var goodsList = dbContext.Goods.Where(p => p.BranchId == setting.BranchId && p.Price > 0 && p.Price <= 10000 && p.IsEnable).OrderByDescending(e => e.SaleNumber).Take(3000).ToList();

            var supplierList = dbContext.Supplier.Where(p => p.IsEnable == true && p.BranchId == setting.BranchId)
                .OrderByDescending(e => e.Id).Take(300).ToList();

            dbContext.Dispose();
            Logger.Debug("开始执行", DateTime.Now.ToLongTimeString());


            var end = false;
            var st = new Stopwatch();
            st.Start();
            Task.Factory.StartNew(() =>
            {
                while (!end)
                {
                    Thread.Sleep(1000);
                    var avg = (int)(statistics.orderCount * 1000 / st.ElapsedMilliseconds);
                    var need = (int)((setting.OperateMsgList.Sum(e => e.DataCount) - statistics.orderCount) / avg);
                    Console.WriteLine($"已完成{statistics.orderCount}个订单,耗时{st.ElapsedMilliseconds}ms,平均{avg}个/s,剩余{need}s");
                }
            });

            var list = new List<Task>();
            // 向purchase表中插入数据 多任务
            for (var i = 0; i < DataFactoryTaskNum; i++)
            {
                var task = new Task(() =>
                {
                    DataFactory(setting.OperateMsgList, setting.BranchId, userList, goodsList, supplierList);
                });
                task.Start();
                list.Add(task);
            }

            Task.WaitAll(list.ToArray());
            end = true;
            st.Stop();
            Console.WriteLine($"执行成功{setting.OperateMsgList.Sum(e => e.DataCount)}条,总耗时{st.ElapsedMilliseconds}ms");
            Console.ReadKey();

        }

        /// <summary>
        ///     数据工厂
        /// </summary>
        /// <param name="opts"></param>
        /// <param name="branchId"></param>
        /// <param name="userList"></param>
        /// <param name="goodsList"></param>
        /// <param name="supplierList"></param>
        private static void DataFactory(List<OperateMsg> opts, int branchId, List<int> userList, List<Goods> goodsList,
            List<Supplier> supplierList)
        {
            foreach (var itemOperate in opts)
            {
                CreateData(itemOperate, branchId, userList, goodsList, supplierList);
            }
        }

        /// <summary>
        ///     生产线
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="branchId"></param>
        /// <param name="userList"></param>
        /// <param name="goodsList"></param>
        /// <param name="supplierList"></param>
        private static void CreateData(OperateMsg setting, int branchId, List<int> userList, List<Goods> goodsList,
            List<Supplier> supplierList)
        {
            var dbContext = new ErpDataBase();
            try
            {
                // 生成订单数 setting.DataCount
                for (var i = 0; i < setting.DataCount / DataFactoryTaskNum; i++)
                {
                    var userId = GetSingleOfList(userList);
                    var supplerIdList = supplierList.Select(p => p.Id).ToList();
                    var supplierId = GetSingleOfList(supplerIdList);
                    // 创建采购单
                    var purchase = CreatePurchase(dbContext, setting, branchId, userId, supplierId);
                    if (purchase != null)
                    {
                        // 创建采购单明细
                        var purchaseDetails = CreatePurchaseDetail(setting, purchase, goodsList);
                        UpdatePurchase(dbContext, purchase, purchaseDetails);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(nameof(CreateData), ex);
            }
            finally
            {
                dbContext.Dispose();
            }
        }

        /// <summary>
        ///     更新采购单
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="purchase"></param>
        /// <param name="purchaseDetails"></param>
        /// <returns></returns>
        private static bool UpdatePurchase(ErpDataBase dbContext, Purchase purchase, List<PurchaseDetail> purchaseDetails)
        {
            if (purchase == null || purchaseDetails.Count == 0)
            {
                return false;
            }
            var moneyNum = purchaseDetails.Sum(p => p.Amount);
            var Num = purchaseDetails.Sum(p => p.Num);
            purchase.SumMoney = moneyNum;
            purchase.ChargeOff = moneyNum;
            purchase.NoTaxMoney = moneyNum;
            purchase.TaxMoney = moneyNum;
            purchase.SaveNum = Num;

            dbContext.Purchase.AddOrUpdate(purchase);
            dbContext.PurchaseDetail.AddRange(purchaseDetails);
            dbContext.SaveChanges();

            return true;
        }

        /// <summary>
        ///     创建采购单
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="setting"></param>
        /// <param name="branchId"></param>
        /// <param name="userId"></param>
        /// <param name="supplierId"></param>
        /// <returns></returns>
        private static Purchase CreatePurchase(ErpDataBase dbContext, OperateMsg setting, int branchId, int userId,
            int supplierId)
        {
            try
            {
                var datetime = GetRandomTime(setting.StarTime, setting.EndTime);

                var purchase = new Purchase
                {
                    BranchId = branchId,
                    PurchaseDate = datetime,
                    AddTime = datetime,
                    UpdateTime = datetime,
                    PutInStoreTime = datetime,
                    EstimateRecieveDate = datetime,
                    AddUserId = userId,
                    IsDelete = false,
                    IsMerge = false,
                    IsBrandPurchase = false,
                    IsChecked = false,
                    Status = "已确认",
                    PayStatus = "未付款",
                    Memo = "20210725add",
                    ThirdPartyId = "20210725add",
                    UserId = userId,
                    StoreId = 1,
                    NeedToPurchaseId = 1,
                    PurchaseType = "供应商直发",
                    SumMoney = 10,
                    SupplierStatus = "未处理",
                    Remark = "",
                    SupplierId = supplierId,
                    ReceiptUserId = userId,
                    ChargeOff = 1,
                    SaveNum = 1,
                    OrderId = 1,
                    TaxMoney = 1,
                    NoTaxMoney = 1
                };

                dbContext.Purchase.Add(purchase);
                dbContext.SaveChanges();
                Interlocked.Increment(ref statistics.orderCount);

                return purchase.Id > 0 ? purchase : new Purchase();
            }
            catch (Exception ex)
            {
                Logger.Error("采购单新增失败", ex);
                return null;
            }
        }

        /// <summary>
        ///     创建采购单明细
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="purchase"></param>
        /// <param name="goodsList"></param>
        /// <returns></returns>
        private static List<PurchaseDetail> CreatePurchaseDetail(OperateMsg setting, Purchase purchase, List<Goods> goodsList)
        {
            var purchaseDtails = new List<PurchaseDetail>();
            var maxAmount = setting.MaxGoodsNum;
            var amountNum = (decimal)0;
            var goodsIds = goodsList.Select(e => e.Id).ToList();
            var index = 0;
            var randomGoodIds = GetRandomGoods(goodsIds, 10);
            goodsList = goodsList.Where(p => randomGoodIds.Contains(p.Id)).ToList();
            var datetime = GetRandomTime(setting.StarTime, setting.EndTime);
            foreach (var goods in goodsList)
            {
                index++;
                var offset = maxAmount - amountNum;
                offset = offset > 0 ? offset : 1;
                var max = offset / goods.Price;
                max = max <= 1 ? 1 : max;
                var num = GetRandomNum(1, (int)max);
                var amount = CalculateOrderAmount(goods.Price, num);
                var noTaxAmount = CalcNoTaxAmountRoundFour(amount, (decimal)0.13);
                var taxRate = (decimal)0.130;
                var taxAmount = CalcTaxAmountRoundFour(amount, noTaxAmount);

                if (index > 1)
                {
                    if (amount + amountNum > maxAmount + 1000)
                    {
                        break;
                    }
                }
                if (amountNum >= maxAmount)
                {
                    break;
                }

                purchaseDtails.Add(new PurchaseDetail
                {
                    PurchaseId = purchase.Id,
                    GoodsId = goods.Id,
                    BillsPrice = amount,
                    InPrice = goods.InPrice * num,
                    DelayDate = datetime,
                    IsEmergency = false,
                    IsCustomGoods = false,
                    IsDonvvSupply = false,
                    IsGift = false,
                    IsTotalAmount = false,
                    Num = num,
                    NoTaxInPrice = CalculateOrderAmount(goods.InPrice, num),
                    Amount = CalculateOrderAmount(goods.Price, num),
                    TaxRate = taxRate,
                    TaxAmount = taxAmount,
                    NoTaxAmount = noTaxAmount,

                    ReceivedNum = 1,
                    SupplierNum = 1,
                    PurchaseStatus = "",
                    DetailMemo = "",
                    OrderDetailId = 1,
                    OldPurchaseId = 1,
                    DonvvBrandId = "1",
                    DonvvBrandName = "",
                    RefundNum = 10,
                    InvoiceType = ""

                });

                amountNum += amount;
            }

            return purchaseDtails;
        }

        /// <summary>
        ///     随机数组下标获取value
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private static int GetSingleOfList(List<int> list)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            var index = GetRandomNum(0, list.Count);

            return list[index];
        }

        /// <summary>
        ///     生成一个随机整数
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private static int GetRandomNum(int min, int max)
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var num = random.Next(min, max);

            return num;
        }

        /// <summary>
        ///     计算金额（四舍五入保留四位小数） jiaqiu
        /// </summary>
        /// <returns></returns>
        private static decimal CalculateOrderAmount(decimal price, int num)
        {
            var amount = price * num;
            amount = Math.Round(amount, 4, MidpointRounding.AwayFromZero);
            return amount;
        }

        /// <summary>
        ///     计算不含税额
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="taxRate"></param>
        /// <returns></returns>
        private static decimal CalcNoTaxAmountRoundFour(decimal amount, decimal taxRate)
        {
            return MathRoundFour(amount / (1 + taxRate));
        }

        /// <summary>
        ///     金额保留4位数
        /// </summary>
        /// <param name="money">金额</param>
        /// <returns></returns>
        private static decimal MathRoundFour(decimal money)
        {
            return Math.Round(money, 4, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        ///     计算税额
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="noTaxAmount"></param>
        /// <returns></returns>
        private static decimal CalcTaxAmountRoundFour(decimal amount, decimal noTaxAmount)
        {
            return MathRoundFour(amount - noTaxAmount);
        }

        /// <summary>
        ///     获取随机商品
        /// </summary>
        /// <param name="list"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private static List<int> GetRandomGoods(List<int> list, int max)
        {
            var ints = new List<int>();
            var num = GetRandomNum(1, max);
            for (var i = 0; i < num; i++) ints.Add(GetSingleOfList(list));
            return ints;
        }

        /// <summary>
        ///     获取随机时间
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static DateTime GetRandomTime(DateTime start, DateTime end)
        {
            var faker = new Faker<Date>()
                .RuleFor(o => o.date, f => f.Date.Between(start, end));

            return faker.Generate(1)[0].date;
        }
    }
}
