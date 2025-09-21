using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// -------------------- Options Pattern --------------------
public class MySettings { public string ApiKey { get; set; } = "12345"; }

// -------------------- Dependency Injection --------------------
public interface IMyService { void DoWork(); }
public class MyService : IMyService { public void DoWork() => Console.WriteLine("Service working"); }

// -------------------- Repository --------------------
public class Post { public string Author { get; set; } = "Tonio"; }
public interface IPostRepo { IEnumerable<Post> GetAll(); }
public class PostRepo : IPostRepo { public IEnumerable<Post> GetAll() => new[] { new Post() }; }

// -------------------- Specification --------------------
public class PostByAuthorSpec { string author; public PostByAuthorSpec(string a) => author = a; public bool IsSatisfied(Post p) => p.Author == author; }

// -------------------- Event Aggregator --------------------
public class UserCreated { public int Id; }
public class EventAggregator
{
    public event Action<UserCreated> UserCreatedEvent;
    public void Subscribe<T>(Action<T> handler) { if (typeof(T) == typeof(UserCreated)) UserCreatedEvent += handler as Action<UserCreated>; }
    public void Publish(UserCreated e) => UserCreatedEvent?.Invoke(e);
}

// -------------------- Mediator --------------------
public interface ICommand { }
public class CreateOrder : ICommand { }
public class Mediator
{
    public Task Send(ICommand c) { Console.WriteLine("Mediator send command"); return Task.CompletedTask; }
    public Task Publish(ICommand c) { Console.WriteLine("Mediator publish event"); return Task.CompletedTask; }
}

// -------------------- Result Pattern --------------------
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }
    private Result(T v) { IsSuccess = true; Value = v; }
    private Result(string e) { IsSuccess = false; Error = e; }
    public static Result<T> Success(T v) => new(v);
    public static Result<T> Failure(string e) => new(e);
}

// -------------------- Null Object --------------------
public interface ILogger { void Log(string msg); }
public class NullLogger : ILogger { public static NullLogger Instance { get; } = new(); public void Log(string msg) { } }

// -------------------- Async/Await --------------------
public class HttpClientMock { public Task<string> GetStringAsync(string url) => Task.FromResult("data"); }

// -------------------- Iterator --------------------
public static class NumbersGenerator { public static IEnumerable<int> Numbers() { yield return 1; yield return 2; } }

// -------------------- Reactive --------------------
public class ObservableMock : IObservable<int>
{
    public IDisposable Subscribe(IObserver<int> observer) { observer.OnNext(42); observer.OnCompleted(); return null!; }
}

// -------------------- Decorator / Middleware --------------------
public class LoggingMiddleware { public void Invoke(string ctx) => Console.WriteLine($"Middleware: {ctx}"); }

// -------------------- Pipeline --------------------
public class Context { public int Data; }
public class Pipeline { List<Action<Context>> steps = new(); public void AddStep(Action<Context> s) => steps.Add(s); public void Execute(Context c) { foreach (var s in steps) s(c); } }

// -------------------- Template Method --------------------
public abstract class Exporter { public void Export(string data) { Open(); Write(data); Close(); } protected abstract void Write(string d); protected void Open() { } protected void Close() { } }
public class CsvExporter : Exporter { protected override void Write(string d) => Console.WriteLine($"CSV: {d}"); }

// -------------------- Program Main --------------------
class Program
{
    static async Task Main()
    {
        Console.WriteLine("---- Options ----");
        var options = Options.Create(new MySettings());
        Console.WriteLine(options.Value.ApiKey);

        Console.WriteLine("---- DI ----");
        var services = new ServiceCollection();
        services.AddTransient<IMyService, MyService>();
        var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IMyService>().DoWork();

        Console.WriteLine("---- Repository & Specification ----");
        var repo = new PostRepo();
        var posts = repo.GetAll();
        var spec = new PostByAuthorSpec("Tonio");
        foreach (var p in posts.Where(p => spec.IsSatisfied(p))) Console.WriteLine($"Post by {p.Author}");

        Console.WriteLine("---- Event Aggregator ----");
        var aggregator = new EventAggregator();
        aggregator.Subscribe<UserCreated>(e => Console.WriteLine($"UserCreated {e.Id}"));
        aggregator.Publish(new UserCreated { Id = 1 });

        Console.WriteLine("---- Mediator ----");
        var mediator = new Mediator();
        await mediator.Send(new CreateOrder());
        await mediator.Publish(new CreateOrder());

        Console.WriteLine("---- Result ----");
        var ok = Result<int>.Success(42);
        var fail = Result<int>.Failure("error");
        Console.WriteLine(ok.IsSuccess ? ok.Value.ToString() : ok.Error);
        Console.WriteLine(fail.IsSuccess ? fail.Value.ToString() : fail.Error);

        Console.WriteLine("---- Null Object ----");
        ILogger log = NullLogger.Instance;
        log.Log("This won't print");

        Console.WriteLine("---- Async/Await ----");
        var http = new HttpClientMock();
        var data = await http.GetStringAsync("url");
        Console.WriteLine(data);

        Console.WriteLine("---- Iterator ----");
        foreach (var n in NumbersGenerator.Numbers()) Console.WriteLine(n);

        Console.WriteLine("---- Reactive ----");
        var observable = new ObservableMock();
        observable.Subscribe(new Observer());

        Console.WriteLine("---- Decorator / Middleware ----");
        var middleware = new LoggingMiddleware();
        middleware.Invoke("Request");

        Console.WriteLine("---- Pipeline ----");
        var pipeline = new Pipeline();
        pipeline.AddStep(c => c.Data++);
        var ctx = new Context { Data = 1 };
        pipeline.Execute(ctx);
        Console.WriteLine(ctx.Data);

        Console.WriteLine("---- Template Method ----");
        var exporter = new CsvExporter();
        exporter.Export("Hello");
    }

    // For Reactive example
    class Observer : IObserver<int>
    {
        public void OnCompleted() => Console.WriteLine("Completed");
        public void OnError(Exception error) => Console.WriteLine(error.Message);
        public void OnNext(int value) => Console.WriteLine($"Received {value}");
    }
}
