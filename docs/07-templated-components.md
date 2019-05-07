# Templated components

Let's refactor some of the original components and make them more reusable. Along the way we'll also create a separate library project as a home for the new components.

## Creating a component library (command line)

We're going to create a new project using the **dotnet** cli in this step since the Blazor Class Library project doesn't yet show up in Visual Studio.

To make a new project using **dotnet** run the following commands from the directory where your solution file exists.

```
dotnet new -i Microsoft.AspNetCore.Blazor.Templates
dotnet new blazorlib -o BlazingComponents
dotnet sln add BlazingComponents
```

This should create a new project called `BlazingComponents` and add it to the solution file. There currently is not a dedicated Blazor Class Libary template, so we'll modify this one in the next step.

## Understanding the library project

Open the project file via *right click* -> *Edit BlazingComponents.csproj*. We're not going to modify anything here, but it would be good to understand a few things.

It looks like:

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <LangVersion>7.3</LangVersion>
    <RazorLangVersion>3.0</RazorLangVersion>
    <IsPackable>true</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <!-- .js/.css files will be referenced via <script>/<link> tags; other content files will just be included in the app's 'dist' directory without any tags referencing them -->
    <EmbeddedResource Include="content\**\*.js" LogicalName="blazor:js:%(RecursiveDir)%(Filename)%(Extension)" />
    <EmbeddedResource Include="content\**\*.css" LogicalName="blazor:css:%(RecursiveDir)%(Filename)%(Extension)" />
    <EmbeddedResource Include="content\**" Exclude="**\*.js;**\*.css" LogicalName="blazor:file:%(RecursiveDir)%(Filename)%(Extension)" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.Browser" Version="3.0.0-preview5-19227-01" />
  </ItemGroup>

</Project>

```

There are a few things here worth understanding. 

Firstly, it's recommended that all Blazor project targets C# version 7.3 or newer (`<LangVersion>7.3</LangVersion>`), Blazor relies on some features added in version 7.3 to make event handlers work correctly.

Next, the `<IsPackable>true</IsPackable>` line makes it possible the create a NuGet package from this project. We won't be using this project as a package in this example, but this is a good thing to have for a class library.

Next, the lines that look like `<EmbeddedResource ... />` give the class library project special handling of content files that should be included in the project. This makes it easier to do multi-project development with static assests, and to redistibute libraries containing static assets. We saw this in action already in the previous step.

Lastly the `<PackageReference />` element adds a package references to the Blazor component model.

## Writing a templated dialog

We are going to revisit the dialog system that is part of `Index` and turn it into something that's decoupled from the application.

Let's think about how a *reusable dialog* should work. We would expect a dialog component to handle showing and hiding itself, as well as maybe styling to appear visually as a dialog. However, to be truly reusable, we need to be able to provide the content for the inside of the dialog. We call a component that accepts *content* as a parameter a *templated component*.

Blazor happens to have a feature that works for exactly this case, it's similar to how a layout works. Recall that a layout has a `Body` parameter, and the layout gets to place other content *around* the `Body`. In a layout, the `Body` parameter is of type `RenderFragment` which is a delegate type that the runtime has special handling for. The good news is that this feature is not limited to layouts. Any component can declare a parameter of type `RenderFragment`.

Let's get started on this new dialog component. Create a new component file named `TemplatedDialog.razor` in the `BlazingComponents` project. Put the following markup inside `TemplatedDialog.razor`:

```html
<div class="dialog-container">
    <div class="dialog">

    </div>
</div>
```

This doesn't do anything yet because we haven't added any parameters. Recall from before the two things we want to accomplish.
1. Accept the content of the dialog as a parameter
1. Render the dialog conditionally if it is supposed to be shown

First, let's add a parameter called `ChildContent` of type `RenderFragment`. The name `ChildContent` is a special parameter name, and is used by convention when a component wants to accept a single content parameter. Next, update the markup to *render* the `ChildContent` in the middle of the markup. It should look like this:

```html
<div class="dialog-container">
    <div class="dialog">
        @ChildContent
    </div>
</div>
```

If this structure looks weird to you, cross-check it with your layout file, which follows a similar pattern. Even though `RenderFragment` is a delegate type, the way to *render* it not by invoking it, it's by placing the value in a normal expression so the runtime may invoke it.

Next, to give this dialog some conditional behavior, let's add a parameter of type `bool` called `Show`. After doing that, it's time to wrap all of the existing content in an `@if (Show) { ... }`. The full file should look like this:

```html
@if (Show)
{
    <div class="dialog-container">
        <div class="dialog">
            @ChildContent
        </div>
    </div>
}

@functions {
    [Parameter] RenderFragment ChildContent { get; set; }
    [Parameter] bool Show { get; set; }
}
```

Do build and make sure that everything compiles at this stage. Next we'll get down to using this new component.

## Adding a reference to the templated library

Before we can use this component in the `BlazingPizza.Client` project, we will need to add a project reference. Do this by adding a project reference from `BlazingPizza.Client` to `BlazingComponents`.

Once that's done, there's one more minor step. Open the `_Imports.razor` in the topmost directory of `BlazingPizza.Client` and add this line at the end:

```html
@using BlazingComponents
```

Now that the project reference has been added, do a build again to verify that everything still compiles.

## Another refactor

We're also going to do another slight refactor to decouple the `ConfigurePizzaDialog` from `OrderState`. This is an idea that we discussed after step 4, and we want to try it to see if it feels better. This will also help work around an issue we discovered while writing this section - Blazor is still under development after all.

Let's add back the `Pizza`, `OnCancel`, and `OnConfirm` parameters. Also move the `AddTopping` and `RemoveTopping` from `OrderState`. The result of all of this is that the `@functions` block of `ConfigurePizzaDialog.razor` should look the same as it did after completing step 2.

```html
@functions {
    List<Topping> toppings { get; set; }

    [Parameter] Pizza Pizza { get; set; }
    [Parameter] EventCallback OnCancel { get; set; }
    [Parameter] EventCallback OnConfirm { get; set; }

    protected async override Task OnInitAsync()
    {
        toppings = await HttpClient.GetJsonAsync<List<Topping>>("toppings");
    }

    void ToppingSelected(UIChangeEventArgs e)
    {
        if (int.TryParse((string)e.Value, out var index) && index >= 0)
        {
            AddTopping(toppings[index]);
        }
    }

    void AddTopping(Topping topping)
    {
        if (Pizza.Toppings.Find(pt => pt.Topping == topping) == null)
        {
            Pizza.Toppings.Add(new PizzaTopping() { Topping = topping });
        }
    }

    void RemoveTopping(Topping topping)
    {
        Pizza.Toppings.RemoveAll(pt => pt.Topping == topping);
    }
}
```

Now we can remove `@inject OrderState OrderState` from the top of `ConfigurePizzaDialog.razor`. 

Lastly, update the rest of this code to use the parameters and remove the lingering usage of `OrderState` from `ConfigurePizzaDialog.razor`. At this point the code should compile, but it will not run correctly because we haven't updated `Index.razor` yet.

----

Recall that our `TemplatedDialog` contains a few `div`s. Well, this duplicates some of the structure of `ConfigurePizzaDialog`. Let's clean that up. Open `ConfigurePizzaDialog.razor`; it currently looks like:

```html
<div class="dialog-container">
    <div class="dialog">
        <div class="dialog-title">
        ...
        </div>
        <form class="dialog-body">
        ...
        </form>

        <div class="dialog-buttons">
        ...
        </div>
    </div>
</div>
```

We should remove the outermost two layers of `div` elements since those are now part of the `TemplatedDialog` component. After removing these it should look more like:

```html
<div class="dialog-title">
...
</div>
<form class="dialog-body">
...
</form>

<div class="dialog-buttons">
...
</div>
```

## Using the new dialog

We'll use this new templated component from `Index.razor`. Open the `Index.razor` and find the block of code that looks like:

```html
@if (OrderState.ShowingConfigureDialog)
{
    <ConfigurePizzaDialog />
}
```

We are going to remove this and replace it with an invocation of the new component. Replace the block above with code like the following:

```html
<TemplatedDialog Show="OrderState.ShowingConfigureDialog">
    <ConfigurePizzaDialog 
        Pizza="OrderState.ConfiguringPizza" 
        OnCancel="OrderState.CancelConfigurePizzaDialog" 
        OnConfirm="OrderState.ConfirmConfigurePizzaDialog" />
</TemplatedDialog>
```

This is wiring up our new `TemplatedDialog` component to show and hide itself based on `OrderState.ShowingConfigureDialog`. Also, we're passing in some content to the `ChildContent` parameter. Since we called the parameter `ChildContent` any content that is placed inside the `<TemplatedDialog> </TemplatedDialog>` will be captured by a `RenderFragment` delegate and passed to `TemplatedDialog`. 

note: A templated component may have multiple `RenderFragment` parameters, what we're showing here is a convenient convention when the caller wants to provide a single `RenderFragment` that represents the *main* content.

At this point it should be possible to run the code and see that the new dialog works correctly. Verify that this is working correctly before moving on to the next step.

## A more advanced templated component

Now that we've done a basic templated dialog, we're going to try something more sophisticated. Recall that the `MyOrders.razor` page has shows a list of orders, but it also contains three-state logic (loading, empty list, and showing items). If we could extract that logic into a reusable component, would that be useful? Let's give it a try.

Start by creating a new file `TemplatedList.razor` in the `BlazingComponents` project. We want this list to have a few features:
1. Async-loading of any type of data
1. Separate rendering logic for three states - loading, empty list, and showing items

We can solve async loading by accepting a delegate of type `Func<Task<List<?>>>` - we need to figure out what type should replace **?**. Since we want to support any kind of data, we need to declare this component as a generic type. We can make a generic-typed component using the `@typeparam` directive, so place this at the top of `TemplatedList.razor`.

```html
@typeparam TItem
```

Making a generic-typed component works similarly to other generic types in C#, in fact `@typeparam` is just a convenient Razor syntax for a generic .NET type.

note: We don't yet have support for type-parameter-constraints. This is something we're looking to add in the future.

Now that we've defined by a generic type parameter we can use it in a parameter declaration. Let's add a parameter to accept a delegate we can use to load data, and then load the data in a similar fashion to our other components.

```html
@functions {
    List<TItem> items;

    [Parameter] Func<Task<List<TItem>>> Loader { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        items = await Loader();
    }
}
```

Since we have the data, we can now add the structure of each of the states we need to handle. Add the following markup to `TemplatedList.razor`:

```html
@if (items == null)
{

}
else if (items.Count == 0)
{
}
else
{
    <div class="list-group">
        @foreach (var item in items)
        {
            <div class="list-group-item">
                
            </div>
        }
    </div>
}
```

Now, these are our three states of the dialog, and we'd like accept a content parameter for each one so the caller can plug in the desired content. We do this by defining three `RenderFragment` parameters. Since we have multiple we'll just give them their own descriptive names instead of calling them `ChildContent`. However, the content for showing an an item needs to take a parameter. We can do this by using `RenderFragment<T>`.

Here's an example of the three parameters to add:

```C#
    [Parameter] RenderFragment LoadingContent { get; set; }
    [Parameter] RenderFragment EmptyContent { get; set; }
    [Parameter] RenderFragment<TItem> ItemContent { get; set; }
```

note: naming a `RenderFragment` parameter with the suffix *Content* is just a convention.

Now that we have some `RenderFragment` parameters, we can start using them. Update the markup we created earlier to plug in the correct parameter in each place.

```html
@if (items == null)
{
    @LoadingContent
}
else if (items.Count == 0)
{
    @EmptyContent
}
else
{
    <div class="list-group">
        @foreach (var item in items)
        {
            <div class="list-group-item">
                @ItemContent(item)
            </div>
        }
    </div>
}
```

The `ItemContent` accepts a parameter, and the way to deal with this is just to invoke the function. The result of invoking a `RenderFragment<T>` is another `RenderFragment` which can be rendered directly.

The new component should compile at this point, but there's still one thing we want to do. We want to be able to style the `<div class="list-group">` with another class, since that's what `MyOrders.razor` is doing. Adding small extensibiliy points to plug in additional css classes can go a long way for reusability.

Let's add another `string` parameter, and finally the functions block of `TemplatedList.razor` should look like:

```html
@functions {
    List<TItem> items;

    [Parameter] Func<Task<List<TItem>>> Loader { get; set; }
    [Parameter] RenderFragment LoadingContent { get; set; }
    [Parameter] RenderFragment EmptyContent { get; set; }
    [Parameter] RenderFragment<TItem> ItemContent { get; set; }
    [Parameter] string ListGroupClass { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        items = await Loader();
    }
}
```

Lastly update the `<div class="list-group">` to contain `<div class="list-group @ListGroupClass">`. The complete file of `TemplatedList.razor` should now look like:

```html
@typeparam TItem

@if (items == null)
{
    @LoadingContent
}
else if (items.Count == 0)
{
    @EmptyContent
}
else
{
    <div class="list-group @ListGroupClass">
        @foreach (var item in items)
        {
            <div class="list-group-item">
                @ItemContent(item)
            </div>
        }
    </div>
}

@functions {
    List<TItem> items;

    [Parameter] Func<Task<List<TItem>>> Loader { get; set; }
    [Parameter] RenderFragment LoadingContent { get; set; }
    [Parameter] RenderFragment EmptyContent { get; set; }
    [Parameter] RenderFragment<TItem> ItemContent { get; set; }
    [Parameter] string ListGroupClass { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        items = await Loader();
    }
}
```

## Using TemplatedList

To use the new `TemplatedList` component, we're going to edit `MyOrders.razor`.

First, we need to create a delegate that we can pass to the `TemplatedList` that will load order data. We can do this by keeping the line of code that's in `MyOrders.OnInitAsync` and changing the method signature. The `@functions` block should look something like:

```html
@functions {
    Task<List<OrderWithStatus>> LoadOrders()
    {
        return HttpClient.GetJsonAsync<List<OrderWithStatus>>("orders");
    }
}
```

This matches the signature expected by the `Loader` parameter of `TemplatedList`, it's a `Func<Task<List<?>>>` where the **?** is replaced with `OrderWithStatus` so we are on the right track.

If you use the `TemplatedList` component now like so:

```html
<TemplatedList>
</TemplatedList>
```

The compiler will complain about not knowing the generic type of `TemplatedList`. The compiler is smart enough to perform type inference like normal C# but we haven't given it enough information to work with.

Adding the `Loader` attribute should fix the issue.

```html
<TemplatedList Loader="@LoadOrders">
</TemplatedList>
```

note: A generic-typed component can have its type-parameters manually specified as well by setting the attribute with a matching name to the type parameter - in this case it's called `TItem`. There are some cases where this is necessary so it's worth knowing.

```html
<TemplatedList TItem="OrderStatus">
</TemplatedList>
```

We don't need to do this right now because the type can be inferred from `Loader`.

-----

Next, we need to think about how to pass multiple content (`RenderFragment`) parameters to a component. We've learned using `TemplatedDialog` that a single `[Parameter] RenderFragment ChildContent` can be set by nesting content inside the component. However this is just a convenient syntax for the most simple case. When you want to pass multiple content parameters, you can do this by nesting elements inside the component that match the parameter names.

For our `TemplatedList` here's an example that sets each parameter to some dummy content:

```html
    <TemplatedList Loader="@LoadOrders">
        <LoadingContent>Hi there!</LoadingContent>
        <EmptyContent>
            How are you?
        </EmptyContent>
        <ItemContent>
            Are you enjoying Blazor?
        </ItemContent>
    </TemplatedList>
```

The `ItemContent` parameter is a `RenderFragment<T>` - which accepts a parameter. By default this parameter is called `context`. If we type inside of `<ItemContent>  </ItemContent>` then it should be possible to see that `@context` is bound to a variable of type `OrderStatus`. We can rename the parameter by using the `Context` attribute:

```html
    <TemplatedList Loader="@LoadOrders">
        <LoadingContent>Hi there!</LoadingContent>
        <EmptyContent>
            How are you?
        </EmptyContent>
        <ItemContent Context="item">
            Are you enjoying Blazor?
        </ItemContent>
    </TemplatedList>
```

Now we want to include all of the existing content from `MyOrders.razor`, so putting it all together should look more like the following:

```html
    <TemplatedList Loader="@LoadOrders" ListGroupClass="orders-list">
        <LoadingContent><text>Loading...</text></LoadingContent>
        <EmptyContent>
            <h2>No orders placed</h2>
            <a class="btn btn-success" href="">Order some pizza</a>
        </EmptyContent>
        <ItemContent Context="item">
            <div class="col">
                <h5>@item.Order.CreatedTime.ToLongDateString()</h5>
                Items:
                <strong>@item.Order.Pizzas.Count()</strong>;
                Total price:
                <strong>£@item.Order.GetFormattedTotalPrice()</strong>
            </div>
            <div class="col">
                Status: <strong>@item.StatusText</strong>
            </div>
            <div class="col flex-grow-0">
                <a href="myorders/@item.Order.OrderId" class="btn btn-success">
                    Track &gt;
                </a>
            </div>
        </ItemContent>
    </TemplatedList>
```

Notice that we're also setting the `ListGroupClass` parameter to add the additional styling that was present in the original `MyOrders.razor`. 

There were a number of steps and new features to introduce here. Run this and make sure that it works correctly now that we're using the templated list.

To prove that the list is really working correctly we can try the following: 
1. Delete the `pizza.db` from the `Blazor.Server` project to test the case where there are no orders
1. Add an `await Task.Delay(3000);` to `LoadOrders` to test the case where we're still loading

## Summary

So what have we seen in this section?

1. It's possible to write components that accept *content* as a parameter - even multiple content parameters
2. Templated components can by used to abstract like showing a dialog, or async loading of data
3. Components can be generic types which makes them more reusable
