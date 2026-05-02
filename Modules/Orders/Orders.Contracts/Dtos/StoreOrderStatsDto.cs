namespace Orders.Contracts.Dtos;

public record StoreOrderStatsDto(
    decimal TotalRevenue,
    OrdersOverviewDto OrdersOverview);

public record OrdersOverviewDto(
    int Pending,
    int Processing,
    int Delivered);
