using Stateless;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyFakexiecheng.Models
{

    public enum OrderStateEnum
    {
        Pending, // 订单已生成
        Processing, // 支付处理中
        Completed, // 交易成功
        Declined, // 交易失败
        Cancelled, // 订单取消
        Refund, // 已退款
    }

    public enum OrderStateTriggerEnum
    {
        PlaceOrder, // 支付
        Approve, // 收款成功
        Reject, // 收款失败
        Cancel, // 取消
        Return // 退货
    }
    public class Order
    {
        public Order()
        {
            StateMachineInit();
        }
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }//update model ApplicationUser 
        public ICollection<LineItem> OrderItems { get; set; }//from shoppingcart,shoppingcaritems -> orderitams
        public OrderStateEnum State { get; set; }
        public DateTime CreateDateUTC { get; set; }
        public string TransactionMetadata { get; set; }//全部转换为字符串，解耦前后端，第三方支付系统

        //最后加入appdbcontext

        StateMachine<OrderStateEnum, OrderStateTriggerEnum> _machine;
        private void StateMachineInit()
        {
            _machine = new StateMachine<OrderStateEnum, OrderStateTriggerEnum>
                (OrderStateEnum.Pending);

            _machine.Configure(OrderStateEnum.Pending)
                .Permit(OrderStateTriggerEnum.PlaceOrder, OrderStateEnum.Processing)
                .Permit(OrderStateTriggerEnum.Cancel, OrderStateEnum.Cancelled);

            _machine.Configure(OrderStateEnum.Processing)
                .Permit(OrderStateTriggerEnum.Approve, OrderStateEnum.Completed)
                .Permit(OrderStateTriggerEnum.Reject, OrderStateEnum.Declined);

            _machine.Configure(OrderStateEnum.Declined)
                .Permit(OrderStateTriggerEnum.PlaceOrder, OrderStateEnum.Processing);

            _machine.Configure(OrderStateEnum.Completed)
                .Permit(OrderStateTriggerEnum.Return, OrderStateEnum.Refund);
        }
    }
}
