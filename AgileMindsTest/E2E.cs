using Microsoft.Playwright;

namespace AgileMindsTest
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class Tests : PageTest
    {

        private async Task LoginAsync()
        {
            await Page.GotoAsync("https://localhost:60001/login");
            await Page.GetByLabel("Username").ClickAsync();
            await Page.GetByLabel("Username").FillAsync("BackEndDev");
            await Page.GetByLabel("Password").ClickAsync();
            await Page.GetByLabel("Password").FillAsync("1234567");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

            await Task.Delay(2000); // Delay for 2 seconds

        }

        [Test]
        public async Task Login()
        {
            await LoginAsync();
            await Page.GotoAsync("https://localhost:60001/");
            await Expect(Page.GetByText("Welcome To SmartSprint SmartSprint simplifies project management with easy")).ToBeVisibleAsync();
            await Expect(Page.GetByText("TD")).ToBeVisibleAsync();
        }

        [Test]
        public async Task SavingChanges()
        {
            await LoginAsync();

            await Page.GotoAsync("https://localhost:60001/");
            await Page.GetByRole(AriaRole.Complementary).GetByRole(AriaRole.Link, options: new() { Name = "Account" }).ClickAsync();

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Account" })).ToBeVisibleAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" })).ToBeVisibleAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" }).ClickAsync();
            await Expect(Page.GetByText("Account details updated!")).ToBeVisibleAsync();
        }


        [Test]
        public async Task WikiVisible()
        {
            await LoginAsync();

            await Page.GotoAsync("https://localhost:60001/");

            await Page.GetByLabel("Toggle Wiki").ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Scrum" })).ToBeVisibleAsync();
        }

        [Test]
        public async Task WikiNames()
        {
            await LoginAsync();

            await Page.GotoAsync("https://localhost:60001/");

            await Page.GetByLabel("Toggle Wiki").ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Scrum" })).ToBeVisibleAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Scrum" }).ClickAsync();
            await Page.GetByRole(AriaRole.Heading, new() { Name = "Scrum Framework Overview" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Scrum Roles" })).ToBeVisibleAsync();
        }

        [Test]
        public async Task CreatingAProjectWithoutProjectName()
        {
            await LoginAsync();

            await Page.GotoAsync("https://localhost:60001/Projects");
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Create New Project" })).ToBeVisibleAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Create New Project" }).ClickAsync();
            await Expect(Page.GetByText("Project Details Project Name")).ToBeVisibleAsync();
            await Page.GetByLabel("Project Name").ClickAsync();

            await Expect(Page.GetByLabel("Project Name")).ToBeVisibleAsync();
            await Expect(Page.GetByLabel("Project Descripion")).ToBeVisibleAsync();
            await Expect(Page.GetByLabel("Project Name")).ToBeEmptyAsync();
            await Expect(Page.GetByLabel("Project Name")).ToBeEmptyAsync();

            await Expect(Page.GetByLabel("Gameify Project")).Not.ToBeCheckedAsync();
            await Expect(Page.GetByLabel("Discord Bot Integration")).Not.ToBeCheckedAsync();
            await Expect(Page.GetByLabel("Canvas Integration")).Not.ToBeCheckedAsync();

            await Expect(Page.GetByText("Project Details Project")).ToBeVisibleAsync();
        }

        [Test]
        public async Task CheckProject()
        {
            await LoginAsync();

            await Page.GotoAsync("https://localhost:60001/Projects");
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "TestingProjectCreation" }).First).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "LeTest" })).ToBeVisibleAsync();
            await Page.GetByRole(AriaRole.Cell, new() { Name = "TestingProjectCreation" }).First.ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Create Task" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Create Sprint" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Add Member" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Ask AI" })).ToBeVisibleAsync();
            await Expect(Page.GetByText("Tasks Not On Sprint")).ToBeVisibleAsync();
        }
    }
}
