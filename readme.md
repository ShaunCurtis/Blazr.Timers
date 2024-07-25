# Primitive Obsession and Race Conditions in Timers

These are really two separate topics, but they fit together in a discussion about how to implement timers.

A common use case is where you want a real-time display of information from a backend remote service.  There's no simple way to implement a notification system, so you use a timer service in the UI to retrieve the data and update the UI regularly.

## Primitive Obsession

How often do you see:

```csharp
int timePeriod = 2000;
```

What does it mean?  You guess it's in milliseconds, but you don't KNOW, until you see it used.  This is a classic case of *primitive obsession*.  It could mean 2000 years or 2000 ticks.

What we need is a value object that removes that ambiguity.  Luckily there's `TimeSpan`.

There are two main ways you get a `TimeSpan`:

1. Constructing one - the most common being `        TimeSpan elapsedTime = 
            new TimeSpan( days, hours, minutes, seconds );
`
 
1. Getting the difference between to date instances.

###  There's more that just TimeDate

In general you should be using `TimeDateOffset` rather than `TimeDate` as it caters for time zones.

If you are only interested in Dates, use `DateOnly`.  They make date equality checking easy.

## One Timer To Rule Them All

On first impression, you would think building and using timers is relatively expensive.  That's not the case.  There's only one timer *service* per AppDomain: called `TimerQueue`.  Timers are simple objects loaded into the timer list in  `TimeQueue`.

All the timer implementations basically load `TimerQueueTimer` instances into the `TimeQueue` with callbacks to invoke when the timer expires.

## Clean Implementation

Take a look that the following code:

```csharp
private TimeSpan _timeSpan = new TimeSpan(0, 0, 5);
private System.Threading.Timer? _timer { get; set; }

protected override async Task OnInitializedAsync()
{
    await GetDataAsync();

    // Kick off the first one off timer
    _timer = new Timer(this.OnTimerElapsed, null, _timeSpan, Timeout.InfiniteTimeSpan);
}

private async void OnTimerElapsed(object? state)
{
    await GetDataAsync();
    // Creates a new one off timer once we have the current data
    // prevents any thread and data conflicts
    _timer = new Timer(this.OnTimerElapsed, null, _timeSpan, Timeout.InfiniteTimeSpan);
    await InvokeAsync(StateHasChanged);
}

private async Task GetDataAsync()
{
    // make call to data pipeline
}

public void Dispose()
{
    _timer?.Dispose();
}
```

Note: 

1. Uses `TimeSpan` to represent the timer interval.  No ambiguity.
1. Uses the basic `System.Threading.Timer`.
1. Starts a one time timer in `OnInitializedAsync`.  `Timeout.InfiniteTimeSpan` tells the `TimerQueue` to consume it once and then remove it from the timer list.

When the Timer expires `TimerQueue` posts the callback to the ThreadPool.  It:

1. Refreshes the data asynchronously.
1. Creates a new one time timer with itself as the callback.
1. Schedules a component render with the Renderer.




