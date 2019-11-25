# Сервис передачи данных для работы с RabbitMq

Эта библиотека предназначена для работы с очередью сообщений RabbitMq и основана на фреймворке [MassTransit](https://github.com/MassTransit/MassTransit). По сути это просто обертка, которая скрывает некоторые механизмы настройки и упрощает работу с фреймворком MassTransit. Однако, базовые знания для работы с RabbitMq необходимы.

## Начало работы

Для начала необходимо установить пакет

```powershell
install-package MassTransit.DataBusService
```

Основной класс - это класс `DataBus`, который очень простой и умеет по сути только запустить/остановить сервис и публиковать сообщения. Все остальное делается в настройках и описывается в конфигурационном файле приложения. Для этого во время установки пакета в конфигурационном файле создается секция `rabbitmq`,

```xml
  <rabbitmq>
  ...
  </rabbitmq>
```

здесь описываются различные параметры настройки, строки подключения, настройки очередей и обработчиков сообщений.

Пример создания объекта и публикации сообщения:

```csharp

    //Message handler definition
    public class Message1Handler: BaseMessageHandler<Message1>
    {
        public override Task MessageHandle(Message1 message)
        {
            return Console.Out.WriteLineAsync(message.Message);
        }
    }

    //Message definition
    public interface Message1
    {
        string Message { get; set; }
    }

    var dataBus = new DataBus();
    dataBus.Start(); //or StartAsync()
    Console.WriteLine("Bus started");
    dataBus.Publish<Message1>(new {Message = "Hello World!"});
    Console.ReadLine();
    dataBus.Stop(); //or StopAsync()

    /*Output
    Bus started
    Hello World!
    */
```

### Параметры настроек

Этот параметр называется `busSettings`.  
Пример:

```xml
<busSettings>
    <add key="workMode" value="RabbitMq">
</busSettings>
```

На данный момент для предусмотрен один параметр `workMode`, который может принимать одно из двух значений `InMemory` и `RabbitMq`. По названию понятно, что этот параметр задает режим работы библиотеки.

### Строка подключения

Задает настройки подключения к серверу RabbitMq.  
Пример:

```xml
<connections>
    <add name="connection1" url="rabbitmq://localhost/" user="admin" password="admin" />
</connections>
```

### Настройки очередей

Здесь можно указать то, как должна обрабатывать сообщения определенная очередь. При этом название очереди должно совпадать с наименованием одной из очереди указанной в настройках обработчиков сообщений.  
Например, для обработчика сообщения

```xml
    <add type="MassTransit_Saga.CreateNewBook.Message1Handler, MassTransit_Saga.CreateNewBook" queue="message1_queue" />
```

указана очередь `message1_queue`, тогда настройка для этой очередь должна иметь примерно такой вид:

```xml
<queues>
    <add name="message1_queue" threadCount="8" prefetchCountToThread="1" threadsByCoreCount="false" />
</queues>
```

где, `name` - имя очереди, `threadCount` - количество потоков обрабатывающих сообщения (значение по умолчанию 1), `prefetchCount` - количество предварительно выбранных сообщений для одного потока, которые шина забирает из очереди за один раз перед их обработкой  (значение по умолчанию 1), `threadsByCoreCount` - флаг, указывающий, что количество потоков должно равняться количеству ядер процессора системы, этот параметр переопределяет параметр `threadCount`.

### Обработчики сообщений

Здесь описываются какие объекты должны участвовать в обработке сообщений. Эти объекты должны напрямую наследоваться от базового класса обработчика сообщений.  
Пример:

```csharp
namespace Examples.MessageHandlers
{
    //Message definition
    public interface Message1
    {
        string Message { get; set; }
    }

    //Message handler definition
    public class Message1Handler : BaseMessageHandler<Message1>
    {
        public override Task MessageHandle(Message1 message)
        {
            return Console.Out.WriteLineAsync(message.Message);
        }
    }
}
```

Пример настройки в конфигурационном файле:

```xml
<mqhandlers>
    <add type="Examples.MessageHandlers.Message1Handler, Examples" queue="message1_queue"/>
</mqhandlers>
```

Здесь для дополнительной настройки очереди нужно создать запись с именем `message1_queue` в секции `queues`.

## Поддержка IOC

Для поддержки зависимостей в обработчиках сообщений необходимо реализовать интерфейс `IBusDependencyResolver`, после этого настроить с помощью статичесокго класса `DependencyResolver`, например:

```csharp

    public interface IBusDependencyResolver
    {
        /// <summary>
        /// Resolve by generic arguments
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        TService Resolve<TService>();

        /// <summary>
        /// Resolve by serviceType
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        object Resolve(Type serviceType);
    }

    public class AutofacBusDependencyResolver: IBusDependencyResolver
    {
        private readonly IContainer _container;

        public AutofacBusDependencyResolver()
        {
            _container = IocConfigure();
        }

        public TService Resolve<TService>()
        {
            return _container.Resolve<TService>();
        }

        public object Resolve(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }

        private static IContainer IocConfigure()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Interface1Implement>().As<IInterface1>();
            builder.RegisterType<Interface2Implement>().As<IInterface2>();

            return builder.Build();
        }
    }

    ...

    static void Main(string[] args)
    {
        DataBus bus = null;
        DependencyResolver.SetDependencyResolver(new AutofacBusDependencyResolver());
        try
        {
            bus = new DataBus("domer");
            bus.Start();

            ...
        }
        finally
        {
            bus?.Stop();
        }
    }

```

## Публикация сообщений

Для публикации сообщений класс `DataBus` реализует интерфейс `IPublisher`:

```csharp

    public interface IPublisher
    {
        Task Publish(object message);

        Task Publish<TMessage>(TMessage message) where TMessage : class;

        Task Publish<TMessage>(object message) where TMessage : class;

        Task Send<TMessage>(TMessage message) where TMessage : class;

        Task Send<TMessage>(object message) where TMessage : class;
    }

```

## Логирование

В DataBusService реализовано логирование событий получения, обработки и возникающих ошибок при обработке сообщений.  
Пример конфигурационного файла для NLog:

```xml

  <targets>
    <target xsi:type="File" name="f" fileName="${basedir}/logs/${shortdate}.log" layout="${longdate}|${uppercase:${level}}|${message}" />
    <target xsi:type="File" name="dbs" fileName="${basedir}/logs/dbs_messhandlers_${shortdate}.log" />
    <target xsi:type="File" name="dbs_error" fileName="${basedir}/logs/dbs_error_${shortdate}.log" layout="${longdate} ${exception:format=toString,stacktrace:innerFormat=toString,stacktrace:maxInnerExceptionLevel=10}" />
  </targets>

  <rules>
    <logger name="*" minlevel="Trace" writeTo="f" /> 
    <logger name="DataBusService" minlevel="Trace" maxlevel="Warn" writeTo="dbs" />
    <logger name="DataBusService" minlevel="Error" writeTo="dbs_error" />
   
  </rules>

```

В приведенном примере первое целевое назначение предназначено для записи всех событий, во второе назначение пишутся логи событий получения/обработки сообщений, в третьем ошибки возникшие при обработки сообщений

Удачи в работе!