namespace test {
	
class Test {	
	    [Fact]
        public void CounterStagesTest() {
        	// Initial rendering
        	var cut = TestHost.AddComponent<Counter>();
        	var expectedHtml = @"<h1>Counter</h1>
                                 <p>Current count: 0</p>
                                 <button class=""btn btn-primary"">Click me</button>";
        	cut.ShouldBe(expectedHtml);

        	// After first click
        	cut.Find("button").Click();
        	cut.GetChangesSinceFirstRender().ShouldHaveSingleTextChange("Current count: 1");

        	// After second click
        	cut.Find("button").Click();
        	cut.GetChangesSinceFirstRender().ShouldHaveSingleTextChange("Current count: 2");
        }
}       
}