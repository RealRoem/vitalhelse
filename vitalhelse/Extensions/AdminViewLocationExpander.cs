using Microsoft.AspNetCore.Mvc.Razor;

public class AdminViewLocationExpander : IViewLocationExpander
{
    public void PopulateValues(ViewLocationExpanderContext context)
    {
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        // Allow views to load from Views/Admin/{Controller}/{View}.cshtml
        var adminLocations = new[]
        {
            "/Views/Admin/{1}/{0}.cshtml",
            "/Views/Admin/Discount/{0}.cshtml",
        };

        // Combine default + custom
        return adminLocations.Concat(viewLocations);
    }
}