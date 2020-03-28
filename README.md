# GIS_Develop
### GIS底层开发

#### 1、 Lesson_1

Lesson_1第一个迷你GIS软件，功能单一，仅有添加点和点击存在的点显示被点击点的属性的功能；

![Lesson_1BasicClass](https://i.loli.net/2020/03/17/E6jrvWOeCmfLHVg.png)

#### 2、Lesson_2

Lesson_2在功能上和Lesson_1没有差别，仅仅是采用了更加完整的类库，使得MyGIS类库具备了可扩充的能力，也拥有了较为完善的框架；

![Lesson_2](https://i.loli.net/2020/03/17/ejdvDogpsPayAV1.png)

#### 3、Lesson_3

Lesson_3屏幕坐标与地图坐标，本次的主要修改是当绘图坐标值超过窗口像素范围时无法看到空间实体的问题；

![Lesson_3](https://i.loli.net/2020/03/17/IeZU7HpsOdGmgNS.png)

屏幕坐标也称绘图窗口坐标，就是再屏幕上绘图涉及的那个窗口部分的坐标：

![屏幕坐标](https://i.loli.net/2020/03/28/e4RKYsUkM8DLohy.png)
> 坐标转换的图解：
>
> ![](https://i.loli.net/2020/03/18/sPp9wOAZTi8VEtl.png)

#### 4、Lesson_4

Lesson_4在lesson_3的时候我们使用四个坐标极值来确定地图范围，但是这样操作有一个比较大的弊端，用户不太好确定输入多少比较合适，在本次修改中主要解决这个问题，提供一种更加便捷的浏览机制；

![Lesson_4](https://i.loli.net/2020/03/17/FJzrhSPQEvc8pV7.png)

#### 5、Lesson_5

Lesson_5读取了点实体的shape文件，定义了layer类；

**在读取点实体之前我们需要了解一下shapefile的文件格式，下图是我简单整理的，详情参考shapefile白皮书**

![shape file](https://i.loli.net/2020/03/17/c8oG1EnbUlRSxZO.png)

#### 6、Lesson_6

Lesson_6本次主要从shapefile中读取线和面；

#### 7、Lesson_7 

实现了读取dbf属性数据；

#### 8、Lesson_8

读写自己的空间数据文件

> Data文件夹里存放了需要用到的演示数据；