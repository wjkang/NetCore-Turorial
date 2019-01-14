服务器内安装Docker，按着文档安装即可。[安装文档](https://docs.docker.com/install/linux/docker-ce/ubuntu/)

![](img/部署到docker/2019-01-14-10-11-36.png)

### 服务器内构建镜像

编译打包后的代码从CI工具发送到服务器后，使用服务器的docker环境构建镜像，并使用最新的镜像创建容器。

![](img/部署到docker/2019-01-14-10-19-59.png)

与之前使用Supervisor自动部署的区别是：

**rebuild.docker.sh**
```bash
#!/bin/bash
cd /publish
cd publish
docker stop citest
docker rm citest
docker rmi citestimage
docker build --rm -t citestimage -f Dockerfile .
docker run --name citest -p 5005:5001 -d citestimage
```
每次执行脚本的时候，停止容器，删除容器，删除旧镜像，构建新镜像，使用最新的镜像创建重新容器。

**Dockerfile**
```
from  microsoft/aspnetcore:2.0.0
COPY . /app
WORKDIR /app
EXPOSE 5001/tcp
ENTRYPOINT ["dotnet", "CiTest.dll"]
```

### CI工具内构建镜像

#### 流程

* CI工具内构建发布代码
* 直接使用CI工具内的Docker环境构建镜像
* 将镜像push到镜像仓库
* 通知服务器拉取新的镜像进行部署

#### 镜像仓库

使用**[Docker Hub](https://hub.docker.com/)**作为镜像仓库

注册账号后，创建一个仓库

![](img/部署到docker/2019-01-14-11-57-55.png)

#### CI工具

CI还是使用**appveyor**，但是环境切换到**Linux**下(因为构建的镜像也是在Linux下部署)，已经默认安装有Docker环境。
![](img/部署到docker/2019-01-14-11-51-08.png)

##### 配置环境变量

![](img/部署到docker/2019-01-14-12-12-21.png)

将**Docker Hub**账号密码配入环境变量中，并且设置nodejs版本

>**appveyor** **Linux**环境下SSH命令有问题，无法执行远程服务器脚本，也就无法通知服务器拉取新的镜像进行部署，所以使用nodejs连接远程服务器并且执行脚本

安装nodejs程序依赖

![](img/部署到docker/2019-01-14-12-18-20.png)