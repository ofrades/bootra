# JavaScript interop

Users of the pizza store can now track the status of their orders in real time. In this session we'll use JavaScript interop to add a real-time map to the order status page that answers the age old question, "Where's my pizza?!?".

## The Map component

Included in the ComponentsLibrary project is a prebuilt `Map` component for displaying the location of a set of markers and animating their movements over time. We'll use this component to show the location of the user's pizza orders as they are being delivered, but first let's look at how the `Map` component is implemented.

Open *Map.razor* and take a look at the code:

```csharp
@using Microsoft.JSInterop
@inject IJSRuntime JSRuntime

<div id="@elementId" style="height: 100%; width: 100%;"></div>

@functions {
    string elementId = $"map-{Guid.NewGuid().ToString("D")}";
    
    [Parameter] double Zoom { get; set; }
    [Parameter] List<Marker> Markers { get; set; }

    protected async override Task OnAfterRenderAsync()
    {
        await JSRuntime.InvokeAsync<object>(
            "deliveryMap.showOrUpdate",
            elementId,
            Markers);
    }
}
```

The `Map` component uses dependency injection to get an `IJSRuntime` instance. This service can be used to make JavaScript calls to browser APIs or existing JavaScript libraries by calling the `InvokeAsync<TResult>` method. The first parameter to this method specifies the path to the JavaScript function to call relative to the root `window` object. The remaining parameters are arguments to pass to the JavaScript function. The arguments are serialized to JSON so they can be handled in JavaScript.

The `Map` component first renders a `div` with a unique ID for the map and then calls the `deliveryMap.showOrUpdate` function to display the map in the specified element with the specified markers pass to the `Map` component. This is done in the `OnAfterRenderAsync` component lifecycle event to ensure that the component is done rendering its markup. The `deliveryMap.showOrUpdate` function is defined in the *content/deliveryMap.js* file, which then uses [leaflet.js](http://leafletjs.com) and [OpenStreetMap](https://www.openstreetmap.org/) to display the map. The details of how this code works isn't really important - the critical point is that it's possible to call any JavaScript function this way.

How do these files make their way to the Blazor app? If you peek inside of the project file for the ComponentsLibrary you'll see that the files in the content directory are built into the library as embedded resources. The Blazor build infrastructure then takes care of extracting these resources and making them available as static assets.

Add the `Map` component to the `OrderDetails` page by adding the following just below the `track-order-details` `div`:

```html
<div class="track-order-map">
    <Map Zoom="13" Markers="@orderWithStatus.MapMarkers" />
</div>
```

When the `OrderDetails` component polls for order status updates, an update set of markers is returned with the latest location of the pizzas, which then gets reflected on the map.

![Real-time pizza map](https://user-images.githubusercontent.com/1874516/51807322-6018b880-227d-11e9-89e5-ef75f03466b9.gif)

## Add a confirm prompt for deleting pizzas

The JavaScript interop code for the `Map` component was provided for you. Next you'll add some JavaScript interop code of your own.

It would be a shame if users accidentally deleted pizzas from their order (and ended up not buying them!). Let's add a confirm prompt when the user tries to delete a pizza. We'll show the confirm prompt using JavaScript interop.

Add a static `JSRuntimeExtensions` class to the Client project with a `Confirm` extension method off of `IJSRuntime`. Implement the `Confirm` method to call the built-in JavaScript `confirm` function.

```csharp
    public static class JSRuntimeExtensions
    {
        public static Task<bool> Confirm(this IJSRuntime jsRuntime, string message)
        {
            return jsRuntime.InvokeAsync<bool>("confirm", message);
        }
    }
```

Inject the `IJSRuntime` service into the `Index` component so that it can be used there to make JavaScript interop calls.

```
@page "/"
@inject HttpClient HttpClient
@inject OrderState OrderState
@inject IUriHelper UriHelper
@inject IJSRuntime JS
```

Add an async `RemovePizza` method to the `Index` component that calls the `Confirm` method to verify if the user really wants to remove the pizza from the order.

```csharp
async Task RemovePizza(Pizza configuredPizza)
{
    if (await JS.Confirm($"Remove {configuredPizza.Special.Name} pizza from the order?"))
    {
        OrderState.RemoveConfiguredPizza(configuredPizza);
    }
}
```

Update the `OnRemoved` parameter on the `ConfiguredPizzaItems` to be a `Func<Task>` so that it supports async.

```csharp
    [Parameter] Func<Task> OnRemoved { get; set; }
```

In the `Index` component update the event handler for the `ConfiguredPizzaItems` to call the new `RemovePizza` method. 

```csharp
@foreach (var configuredPizza in OrderState.Order.Pizzas)
{
    <ConfiguredPizzaItem Pizza="configuredPizza" OnRemoved="@(() => RemovePizza(configuredPizza))" />
}
```

Run the app and try removing a pizza from the order.

![Confirm pizza removal](https://user-images.githubusercontent.com/1874516/51843485-06f76600-230b-11e9-91e6-517f6d78f13c.png)

Next up - [Templated components](07-templated-components.md)
