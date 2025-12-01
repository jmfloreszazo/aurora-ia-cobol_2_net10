using TechTalk.SpecFlow;

namespace CardDemo.Tests.SpecFlow;

[Binding]
public class Hooks
{
    private readonly TestContext _context;

    public Hooks(TestContext context)
    {
        _context = context;
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        // Reset context to ensure fresh database state for each scenario
        _context.Reset();
    }

    [AfterScenario]
    public void AfterScenario()
    {
        // Cleanup is handled by TestContext.Dispose()
    }
}
