
#### 组件名称：Xye.Grpc.Consul
#### 使用说明
###### 支持平台：.net45;.netstandard2.0
###### 安装：Install-Package Xye.Grpc.Consul 或 直接nuget安装
###### 主要适用：Grpc客户端。
###### 作用：
* 1）支撑自动发现服务或者直连服务
* 2）客户端负载均衡（目前只支持普通哈希算法，后续可以扩展加入权重值，如有必要的话）

#### 配置文件说明
consul服务配置文件格式（适用于客户端或服务端）
文件名【ConsulService.json】用于下面接入使用（文件名可随意修改）
```javascript
 {
  "ConsulServerConfig": {
    "Host": "192.168.44.191", //正式版地址http://consul.comfun.com/
    "Port": "31949", //8500
    "DaemonSleep": 1,
    "BlackTime": 60 
  }
}
```

##### 字段说明
* 1、Host，consul服务地址，IP或者域名
* 2、Port，consul服务端口号
* 3、DaemonSleep，守护线程睡眠时间，单位秒（默认1秒，且至少1秒）该字段只对客户端使用时起作用
* 4、BlackTime，黑名单 禁用时间，单位秒（默认90秒） 只对服务发现起作用，如果直接连接则不会加入黑名单

grpc服务配置格式（适用于客户端）
#### 服务发现配置
文件名【GrpcServices.json】用于下面接入使用（文件名可随意修改）
```javascript
 {
  "GrpcServices": [
    {
      "Name": "Xye.Grpc.TestService",
      "ServiceName": "xyegrpctestservice",
      "IsDiscovery": true,
      "MaxRetry": 2
    },
    {
      "Name": "Xye.Grpc.TestService2",
      "ServiceName": "xyegrpctestservice2",
      "IsDiscovery": true,
      "MaxRetry": 2
    }
  ]
}
```

直接连接配置
文件名【GrpcServices.json】用于下面接入使用（文件名可随意修改）
```javascript
 {
  "GrpcServices": [
    {
      "Name": "Xye.Grpc.TestService",
      "ServiceName": "xyegrpctestservice",
      "Host": "192.168.44.15",
      "Port": 1121,
      "IsDiscovery": false,
      "MaxRetry": 2
    },
    {
      "Name": "Xye.Grpc.TestService",
      "ServiceName": "xyegrpctestservice",
      "Host": "192.168.44.191",
      "Port": 31996,
      "MaxRetry": 2
    }
  ]
}
```

##### 字段说明
* 1、Name，服务名称，由PB文件中【命名空间.服务名】组成
* 2、ServiceName，服务名称，实际为docker镜像名称，一般格式为【服务代号_环境】，比如：
    * 1）正式环境 cfgrantservice 或者 cfgrantservice_online
    * 2）预发布环境 cfgrantservice_pre
    * 3）服务名称全部小写
* 3、IsDiscovery，是否自定发现服务。当IsDiscovery=true时表示服务发现，Host和Port可以不用配置
* 4、Host，Grpc服务绑定地址（IP或域名）
* 5、Port，Grpc服务绑定端口号
* 6、MaxRetry，最大重试次数。当连接Grpc服务发生异常时可重试次数
* 7、特别注意，ServiceName直接影响注册consul里的服务获取；Name直接影响客户端使用服务的初始化。两者一定不能配置错！
* 8、以上ServiceName和Name的配置方式可以看出。一个ServiceName可以有多个不同Name的服务
grpc服务注册配置格式（适用于服务端，非容器部署）
文件名【GrpcServerRegister.json】用于下面接入使用（文件名可随意修改）
```javascript
{
  "XyeGrpcServerRegister": {
    "ServiceName": "xyegrpcservice",
    "Host": "10.0.75.1",
    "Port": 1121,
    "EnableTagOverride": true,
    "Tags": [
      "111",
      "222"
    ],
    "Meta": {
      "meta1": "1",
      "meta2": "2",
      "weights": "1"
    }
  }
}
```

##### 具体说明
* 1、ServiceName，服务名，一般为【服务代码_环境】，如上介绍
* 2、Host，宿主，服务绑定的宿主; 如果不填写 默认会读宿主机IP。读取IP顺序 192.-> 172. -> 10. -> 其他
* 3、Port，端口，服务监听的端口
* 4、EnableTagOverride，是否重写tag，作用于consul
* 5、Tags，作用于consul
* 6、Meta，可以配置权重值，当weights=0，则会自动下线

#### 基于CoC（Convention over Configuration，惯例优于配置）原则，以上所有为我们约定好的配置，作为惯例，避免出现不必要的尴尬！
平台接入
##### 客户端接入
###### .net45平台
全局注入
```javascript
ConsulDI.AddConsulDiscovery(c =>
{
    c.ConsulServerConfigPath = "ConsulService.json"; //添加consul服务配置文件
    c.GrpcServiceConfigPath = "GrpcServices.json"; //添加Grpc服务配置文件
}
, i => { i.Add(new TimingRecordInterceptor()); } //加入拦截器
, (msg, ex) => {
    Console.WriteLine(msg); //日志输出配置
}
);
```

客户端调用
```javascript
var testClient = new ClientFactory<CFGrantBoxGrpc.TestService.TestServiceClient>()
                .Get();
var result = testClient.GetIP(new GetIPRequest());
if (result != null)
{
  //获取结果，进行业务处理
}
```

##### .netstandard2.0平台
全局注入
```javascript
ConfigurationBuilder configBuild = new ConfigurationBuilder();
configBuild
    .AddJsonFile("ConsulService.json", true, true) //添加consul服务配置文件
    .AddJsonFile("GrpcServices.json", true, true) //添加Grpc服务配置文件
    ;
IConfiguration config = configBuild.Build();
IServiceCollection services = new ServiceCollection();
services
    .AddTransient<Interceptor, TimingRecordInterceptor>() //注入Grpc拦截器，可选
    .AddConsulDiscovery(config) //添加consul服务发现
    ;
_serviceProvider = services.BuildServiceProvider();
ConsulDI.UseConsulDiscovery(_serviceProvider, (msg, ex) =>
{
    Console.WriteLine(msg); //日志输出配置
}); //使用consul服务发现
```

客户端调用
```javascript
var testClient = _serviceProvider.GetRequiredService<IClientFactory<CFGrantBoxGrpc.TestService.TestServiceClient>>().Get(); //从容器中获取
var result = testClient.GetIP(new GetIPRequest());
if (result != null)
{
    //获取结果，进行业务处理
}
```

#### 服务端接入
##### 如果部署在docker容器中直接可使用【Registrator】注册器实现
###### 好处：不用侵入代码
##### Registrator（部署于docker容器中）
###### Registrator，注册器，监控docker容器的变化，如，启动，停止等。根据这些变化，获取容器的名称，镜像名，宿主IP和端口，注册或反注册到consul服务端。
##### 介绍
##### 开源地址：https://github.com/gliderlabs/registrator
##### 文档：
* https://gliderlabs.github.io/registrator/latest/
* https://gliderlabs.github.io/registrator/latest/user/run/

##### 安装格式：
```javascript
docker run [docker options] gliderlabs/registrator[:tag] [options] <registry uri>
```

##### 使用的命令如下：
```javascript
localIpUrl=
consulUrl=
hostIp=$( curl $localIpUrl )
docker run -d  \
--name=registrator \
--net=host \
--volume=/var/run/docker.sock:/tmp/docker.sock  \
gliderlabs/registrator:latest  -cleanup -internal=false -ip=$hostIp  consul://$consulUrl
```

##### 命令说明
* 1、internal，使用公开端口替换发布端口，false则使用了宿主机端口；true则使用容器内端口
* 2、ip，用于注册服务的强制IP地址，比如上面命令可以指定宿主机的ip
* 3、注册到consul，则注册的url为consul://url

非部署于docker容器
###### .net45平台
全局注入
```javascript
ConsulDI.AddConsulRegister(c =>
{
    c.ConsulServerConfigPath = "ConsulServer.json";
    c.GrpcServerRegisterPath = "GrpcServerRegister.json";
});
```
应用启动注册服务
```javascript
ServiceRegisterFactory.Instance.RegisterService();
```
应用释放反注册服务
```javascript
ServiceRegisterFactory.Instance.UnregisterService();
```
.netstandard2.0平台
全局注入
```javascript
ConfigurationBuilder configBuild = new ConfigurationBuilder();
configBuild
    .AddJsonFile("Config/Complex/GrpcServerRegister.json", true, true) //添加服务注册配置
    .AddJsonFile("Config/Complex/ConsulServer.json", true, true)  //添加consul服务配置文件
    ;
IConfiguration config = configBuild.Build();
IServiceCollection services = new ServiceCollection();
services
    .AddConsulRegister(config) //添加consul注册
    ;
```
#### 注册和反注册服务
```javascript
//从容器中注册工厂
var registorFac = _serviceProvider.GetRequiredService<ServiceRegisterFactory>(); 
//应用启动注册服务
registorFac.RegisterService();
//应用释放反注册服务
registorFac.UnregisterService();
```