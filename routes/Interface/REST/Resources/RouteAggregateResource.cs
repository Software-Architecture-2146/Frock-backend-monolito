
namespace Frock_backend.routes.Interface.REST.Resources
{
    public record RouteAggregateResource
    (
        int id,
        double price,
        int frequency,
        int duration,
        List<StopInRoutesResource> stops,
        List<ScheduleResource> schedules
    );
}
