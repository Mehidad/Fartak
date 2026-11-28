using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Delivery.Events
{
    // ثبت/تغییر اطلاعات حمل (شرکت حمل و پلاک خودرو)
    public sealed record DeliveryCarrierSetEvent(
        long deliveryId,
        long caseId,
        string? oldCarrier,
        string? newCarrier,
        string? oldVehiclePlate,
        string? newVehiclePlate) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
