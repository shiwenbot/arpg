## MouseManager

1. SetCursorTexture()实现了鼠标悬浮在不同物体上时会有不同贴图
   利用了 Unity 的 RaycastHit 从摄像机发出射线，Physics.Raycast 判断射线是否与 collier 相交

2. MouseControl()实现了点击鼠标左键后移动或者攻击敌人
   利用了 event，根据不用的情况 Invoke 不同的事件，这些事件要调用的方法在 PlayController 类中注册

## PlayerController

1. Start()中把对应的方法都注册到 event 中
2. MoveToAttackTarget()实现了人物移动到敌人处并攻击
   TODO: 为什么要用协程呢？是因为方便玩家在人物还没有到达上一个目标点时就更新了新的目标点吗？

注意：使用了 NavMeshAgent 的 isStopped 来控制玩家是否可以移动

## EnemyController

1. 怪物有几种状态 GUARD, PATROL, CHASE, DEAD，为了确保行为正常以及动画的正确转换使用了几个布尔值，isWalk，isChase，isFollow，isDead
2. 怪物的行为：
   GUARD：站岗，在追击玩家失败以后会返回到原站岗的地方
   PATROL：在一个设定的范围内随机巡逻，并会在每次到达目标地时巡视几秒
   CHASE： findPlayer()方法判断是否发现敌人，发现了会追击，移速加快，脱战后返回上一个状态，追上玩家后会攻击
   DEAD： 死亡
3. GetNewWayPoint()实现了为怪物创建随机的新目标点，利用了 NavMesh.SamplePosition()方法确保目标点时可以到达的，否则怪物会卡住
4. Hit()方法插入到了怪物的攻击动画的特定一帧中，也就是说在攻击动画播放到这一帧后会调用

## CharacterStats

这个类会处理和数据更改相关的所有逻辑

1. 会从 Data_SO 读取到数据
2. CurrentDamage()用于判断是否暴击，TakeDamage()计算攻击后的血量以及播放暴击动画

## Singleton

这个类是一个泛型类的单例模式
