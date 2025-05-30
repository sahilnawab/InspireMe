namespace InspireMe.Contracts
{
    public interface IViewRenderService
    {
        Task<string> RenderViewToStringAsync(string viewPath, object model);

    }
}
