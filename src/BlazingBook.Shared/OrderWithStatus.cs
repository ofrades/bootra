using System;
using System.Collections.Generic;

namespace BlazingBook {
    public class OrderWithStatus {
        public readonly static TimeSpan PreparationDuration = TimeSpan.FromSeconds(5);
        public readonly static TimeSpan DeliveryDuration = TimeSpan.FromSeconds(10);
        public Order Order { get; set; }
        public string StatusText { get; set; }
        public static OrderWithStatus FromOrder(Order order) {
            string statusText;
            var dispatchTime = order.CreatedTime.Add(PreparationDuration);

            if (DateTime.Now < dispatchTime) {
                statusText = "Preparing";
            } else if (DateTime.Now < dispatchTime + DeliveryDuration) {
                statusText = "Out for delivery";
            } else {
                statusText = "Delivered";
            }

            return new OrderWithStatus {
                Order = order,
                StatusText = statusText,
            };
        }
    }
}