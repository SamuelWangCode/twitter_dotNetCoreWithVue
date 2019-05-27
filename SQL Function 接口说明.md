# SQL Function 接口说明

此文档由c#小组（杨紫超，周宇东，魏敬杰）负责编写。
SQL小组通过阅读此文档，编写相应的SQL脚本。
SQL脚本文件统一放置在根目录下的SQLs文件夹中。
所有函数定义在***create_functions.sql*** 文件中。



**我们的项目必定胜利**

## 接口规范
* 函数定义

	> * 使用Oracle SQL function来定义接口，function定义语句统一为create or replace function ...

* 函数命名

	> * 函数名以FUNC_开头，全大写，各单词之间以下划线分隔开来

* 返回值

	> * 函数返回一个INTEGER类型的变量state作为状态码，表示函数执行情况，各状态码含义如下：
	> * state=0 代表函数执行失败
	> * state=1 代表函数执行成功
	> * 如需其他含义的状态码可在经小组协商同意后，添加至该文档

* 输入参数

	> * 输入参数个数不定，具体数目由各接口文档规定
	> * 输入参数保证为Oracle数据库内置的基本类型
	> * 输入参数保证显式指明为in模式，且不存在 in out模式
	> * 输入参数在参数列表中位置保证处于输出参数（如输出参数存在）前

* 输出参数

	> * 输出参数保证只有0个或1个
	> * 输出参数类型保证为Oracle数据库内置基本类型或数据表
	> * 输出参数保证显式指明为out模式，且不存在 in out模式
	> * 输出参数在参数列表中位置保证处于输入参数（如输入参数存在）后
	> * 对于输出参数为数据表的函数，输出参数类型应为sys\_refcursor

* 已完成

  > * 标记该函数是否已经实现，方便SQL编写者区分
  >
  > * 若完成请数据库小组成员标注完成者姓名，如：   
  >
  >   已完成：是
  >
  >     完成者：xxx于 2019-xx-xx

* 异常处理

	> * SQL函数编写者应在定义函数内部时对可能出现的错误进行处理，尽量避免应函数执行错误而抛出异常
	> * SQL函数编写者应以状态码的值表示异常状态

* 注意事项

  > * 若使用select into语句来对单值输出参数赋值时应考虑到检索结果为空或多项结果的情况
  >
  > * 对于位模式mode可以使用bitand函数来判断要进行的步骤
  >
  > * 小组成员可自行将编写过程中踩过的坑添加到注意事项中
  >
  > * 补充注意事项1：在函数内部存在DML操作（尤其时插入删除等）时，会产生ORA-14551: 无法在查询中执行 DML 操作 .”错误，该错误产生原因在于主事务和自治事务的区别，具体请自行了解，这里提供一种解决方法：使用自治事务。在函数声明部分加入这句话
  >
  > PRAGMA AUTONOMOUS_TRANSACTION;并在最后 COMMIT 提交DML操作。
  >
  > 详情见下面样例，相关资料请参考：<https://blog.csdn.net/gigiouter/article/details/7616627>
  > * 补充注意事项2：在一个sql文件中创建多个函数时需要使用分隔符/来分隔不同函数
  >
  > * 使用函数时可以参照以下样例
  >
  >   不带输出参数：
  >
  >   ~~~sql
  >   select func_user_sign_up('xx', 'xx', 'xx')from dual;
  >   ~~~
  >
  >   存在输出参数（可以用一个块来解决，不排除有更简洁的方式）：
  >
  >   ~~~sql
  >   declare
  >   state INTEGER;
  >   user_id INTEGER;
  >   begin
  >   state:=func_send_message('I love you', 0, 3, 0, user_id);
  >   end;
  >   ~~~
  >
  >   
  >
  > 

* 接口样例

	~~~sql
	 create or replace 
	 function FUNC_SEARCH_TOPIC_BY_HEAT(heat in INTEGER, search_result out sys_refcursor)
	 return INTEGER
	 is
	 result_size INTEGER :=0;
	 state INTEGER :=1;
  
    begin 
    select count(*) into result_size from TOPIC where TOPIC_HEAT=heat;
    open search_result for select * from TOPIC where TOPIC_HEAT=heat;
  
    if result_size=0 then
    	state :=0;
    else 
	 	state :=1;
	 end if;
	
	 return state;
	 end;
	~~~
	
	
	
	该代码块定义了一个根据热度heat来获取TOPIC表的函数

> * 使用open for语句将查询结果装载入sys_refcursor类型的输出参数中
> * 当没有查询结果时返回0
> * 查询成功时返回1

 * 接口样例（插入）

   ~~~sql
   create or replace function 
   FUNC_USER_SIGN_UP(email in VARCHAR, nickname in VARCHAR, password in VARCHAR)
   return INTEGER
   is
   PRAGMA AUTONOMOUS_TRANSACTION;
   
   state INTEGER :=1;
   temp_user_id INTEGER :=0;
   register_time VARCHAR2(30) ;
   set_introduction VARCHAR2(255) :='The man is lazy,leaving nothing.';
   begin
   select to_char(sysdate, 'yyyy-mm-dd HH24:MI:SS') into register_time from dual;
   
   insert into USER_PUBLIC_INFO
   (USER_ID, USER_NICKNAME, USER_REGISTER_TIME, USER_SELF_INTRODUCTION, USER_FOLLOWERS_NUM, USER_FOLLOWS_NUM)
   values(SEQ_USER_PUBLIC_INFO.nextval, nickname, register_time, set_introduction, '0', '0');
   
   select USER_ID into temp_user_id from USER_PUBLIC_INFO
   where USER_NICKNAME = nickname;
   
   insert into USER_PRIVATE_INFO(USER_ID, USER_EMAIL, USER_PASSWORD)
   values(temp_user_id, email, password);
   
   commit;
   return state;
   
   end;
   ~~~

   


## 需求接口
### 1.FUNC\_CHECK\_USER\_EMAIL\_EXIST(email in VARCHAR)
* 接口功能：检查用户Email是否存在于数据库中
* 返回值：用户Email存在于数据库时返回1，不存在时返回0
* 输入参数：

	* email：VARCHAR类型，存放待检查的Email

* 输出参数：无

* 已完成：是

  完成者：周宇东于2019-05-22

### 2.FUNC\_SHOW\_MESSAGE\_BY\_ID(message\_id in INTEGER, result out sys_refcursor)
* 接口功能：通过给定的推特ID来查询该推特的详细信息

* 返回值：查询成功返回1，失败返回0

* 输入参数：
	
* message_id：INTEGER类型，表示要查询的消息的ID
	
* 输出参数：
	
* result：sys_refcursor类型，用户信息，table属性为(message\_id, message\_content, message\_create\_time, message\_agree\_num, message\_transpond\_num, message\_comment\_num, message\_view\_num, message\_has\_image, message\_is\_transpond, message_sender\_user\_id, message\_heat, message\_transpond\_message\_id, message\_image\_count)。在5月25日更新后的message表里不再具有message\_transpond\_message\_id这个属性，若该条推特不为转发，该属性可返回-1值，若该条推特是转发，则照旧返回转发来源的ID。
	
* 已完成：否


### 3.FUNC\_SHOW\_HOME\_MESSAGE\_BY\_RANGE(user\_id in INTEGER, rangeStart in INTEGER, rangeLimitation in INTEGER, search\_result out sys_refcursor)
* 接口功能：通过给定的用户ID，索引起点和查询范围来查询某用户发布过的推特

* 返回值：查询成功返回1，失败返回0

* 输入参数：
	* user\_id：INTEGER类型，表示要查询的用户的ID
	* rangeStart：INTEGER类型，表示所查询的推特的范围是从哪一个索引开始的
	* rangeLimitation：INTEGER类型，表示要查询多少条推特

* 输出参数：
	
* search\_result：sys_refcursor类型，用户信息，table属性为(message\_id, message\_content, message\_create\_time, message\_agree\_num, message\_transpond\_num, message\_comment\_num, message\_view\_num, message\_has\_image, message\_is\_transpond, message_sender\_user\_id, message\_heat, message\_transpond\_message\_id, message\_image\_count)
	
* 已完成：否


### 4.FUNC\_SEND\_MESSAGE(message\_content in VARCHAR2, message\_has\_image in INTEGER, user\_id in INTEGER, message\_image\_count in INTEGER, message\_id out INTEGER)
* 接口功能：发布新的推特，通过给出推特内容，推特是否含图，推特图的数量，发布者的ID来保存新推特，并输出该条推特的ID

* 返回值：发布成功返回1，失败返回0

* 输入参数：
	* message\_content：VARCHAR2类型，表示发布的推特的内容（280字符以内）
	* message\_has\_image：INTEGER类型，表示推特是否含图，1为含图，0为不含图
	* user\_id：INTEGER类型，表示发布该推特的用户的ID
	* message\_image\_count：INTEGER类型，表示该推特含图的数量，不含图则表示为0

* 输出参数：
	
* message\_id：INTEGER类型，表示新建的该推特的ID
	
* 已完成：是

  完成者：王笑天于2019-05-27

### 5.FUNC\_TRANSPOND\_MESSAGE(message\_content in VARCHAR2, message\_source\_is\_transpond in INTEGER, message\_sender\_user\_id in INTEGER, message\_transpond\_message\_id in INTEGER, message\_id out INTEGER)
* 接口功能：转发一个已有的推特，并输出这个转发的新推特的ID值

* 返回值：转发成功返回1，失败返回0

* 输入参数：
	* message\_content：VARCHAR2类型，表示新的转发推特的内容（280字符以内）
	* message\_source\_is\_transpond：INTEGER类型，表示转发的来源推特是否也是转发的，1为是，0为否
	* message\_sender\_user\_id：INTEGER类型，表示发布这个转发推特的用户的ID
	* message\_transpond\_message\_id：INTEGER类型，表示这个推特所转发的直接来源推特的ID

* 输出参数：
	
* message\_id：INTEGER类型，表示新创建的这个转发的ID
	
* 已完成：否


### 6.FUNC\_ADD\_TOPIC(topic\_content in VARCHAR2, message\_id in INTEGER)
* 接口功能：为指定话题添加一条推特，若该话题已存在，则热度+1，若该话题不存在则将它创建

* 返回值：添加成功返回1，失败返回0

* 输入参数：
	* topic\_content：VARCHAR2类型，表示话题的内容
	* message\_id：INTEGER类型，表示添加到这个话题的推特ID

* 输出参数：无

* 已完成：否


### 7.FUNC\_DELETE\_MESSAGE(message\_id in INTEGER, message\_has\_image out INTEGER)
* 接口功能：删除指定ID的推特，并且删除这条推特所包含的评论。需要注意的是，如果该推特包含了话题，对推特的删除并不会又减少话题的热度（也就是删除推特，话题热度不变，微博好像是这么设定的）。需要输出删除的这条推特是否含图。

* 返回值：删除成功返回1，失败返回0

* 输入参数：
	
* message\_id：INTEGER类型，表示要删除的推特ID
	
* 输出参数：
	
* message\_has\_image： INTEGER类型，表示删除的这条推特是否含图
	
* 已完成：否


### 8.FUNC\_USER\_SIGN\_UP(email in VARCHAR, nickname in VARCHAR, password in VARCHAR)
* 接口功能：通过给定的用户信息向数据库添加新用户

* 返回值：注册成功返回1，失败返回0

* 输入参数：
	* email：VARCHAR类型，存放用户Email
	* nickname：VARCHAR类型，存放用户昵称
	* password：VARCHAR类型，存放用户密码

* 输出参数：无

* 已完成：是

  完成者：王笑天于2019-05-18

### 9.FUNC\_USER\_SIGN\_IN\_BY\_EMAIL(email in VARCHAR, password in VARCHAR, user_id out INTEGER)
* 接口功能：通过用户Email和密码判断是否登录成功
* 返回值：登录成功返回1，失败返回0
* 输入参数：
	* email：VARCHAR类型，存放用户Email
	* password：VARCHAR类型，存放用户密码

* 输出参数：
	* user_id：登录用户的id值

* 已完成：否

### 10.FUNC\_SET\_USER\_INFO(nickname in VARCHAR, password in VARCHAR,self\_introduction in VARCHAR, realname in VARCHAR, gender in VARCHAR, user\_id in INTEGER, mode in INTEGER)
* 接口功能：修改用户id为user_id的用户的个人信息
* 返回值：修改成功返回1，失败返回0
* 输入参数：
	* nickname：VARCHAR类型，存放用户昵称
	* self\_introduction：VARCHAR类型，存放用户个人介绍
	* password：VARCHAR类型，存放用户密码
	* realname：VARCHAR类型，存放用户真实姓名
	* gender：VARCHAR类型，存放用户性别
	* user\_id：INTERGER类型，待修改用户的user_id
	* mode：INTERGER类型，判断修改哪几个信息，数值范围为1~31，当mode&(1<<i)==(1<<i)时，代表修改第i+1个输入参数所代表的信息

* 输出参数：无
* 已完成：是

  完成者：周宇东于2019-05-22

### 11.FUNC\_SET\_MAIN\_AVATAR(user\_id in INTEGER, avatar\_id in INTEGER)
* 接口功能：设置用户的主要头像
* 返回值：修改成功返回1，失败返回0
* 输入参数：
	* user\_id：INTERGER类型，待修改用户的user_id
	* avatar\_id：INTEGER类型，用户选择的头像

* 输出参数：无
* 已完成：否

### 12.FUNC\_GET\_USER\_AVATAR(user\_id in INTEGER, avatar\_id out INTEGER)
* 接口功能：通过用户id获取用户主要头像的id
* 返回值：获得头像成功返回1，失败返回0
* 输入参数：
	* user\_id：INTERGER类型，用户id
* 输出参数：
	* avatar\_id：INTEGER类型，用户的头像id
* 已完成：否

### 13.FUNC\_GET\_USER\_PUBLIC\_INFO(user\_id in INTEGER, info out sys_refcursor)
* 接口功能：通过用户id获取用户公开信息
* 返回值：获得成功返回1，失败返回0
* 输入参数：
	* user\_id：INTERGER类型，用户id
* 输出参数：
	* info：sys_refcursor类型，用户信息，table属性为(user\_id,user\_nickname,user\_register\_time,user\_avatar\_image\_id,user\_self\_introduction,user\_followers\_num,user\_follows\_num)
* 已完成：否

** 以下更新于5.8 by 杨紫超 **

### 14.FUNC\_ADD\_RELATION(follower_id in INTEGER, be_followed_id in INTEGER)
* 接口功能：给定关注者id follower_id 以及被关注者id be_followed_id，添加一个从关注者到被关注者的关系。同时被关注者的“被关注数”需要+1，关注者的“关注别人的数量"需要+1
* 返回值：同上
* 输入参数：
	* follower\_id: INTEGER类型， 关注者id
	* be\_followed\_id: INTEGER类型，被关注者id
* 输出参数：无
* 已完成：否

### 15.FUNC\_REMOVE\_RELATION(follower\_id in INTEGER, be\_followed\_id in INTEGER)
* 接口功能：给定关注者id follower_id 以及被关注者id be_followed_id，删除一个从关注者到被关注者的关系。同时被关注者的“被关注数”需要-1，关注者的“关注别人的数量"需要-1
* 返回值：同上
* 输入参数：
	* follower\_id: INTEGER类型， 关注者id
	* be\_followed\_id: INTEGER类型，被关注者id
* 输出参数：无
* 已完成：否

### 16.FUNC\_QUERY\_FOLLOWING\_LIST(user\_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search\_result in INTEGER)
* 接口功能：给定用户id:user\_id，查找出他关注的人的，从startFrom开始，长度为limitation的user信息列表，按照时间降序排序
* 返回值：同上
* 输入参数：
	* user\_id: INTEGER类型，被查找的人的id
	* startFrom: INTEGER类型，查找的起点
	* limitation: INTEGER类型，查找结果的长度
* 输出参数：
	* search\_result: sys_refcursor类型。用户的简单信息表格，包括用户的user\_id，用户昵称user\_nickname，用户的使用中头像avatar_id。table的属性为
	(user\_id, user_nickname, avatar_id)
* 已完成：否

### 17.FUNC\_QUERY\_FOLLOWERS\_LIST(user\_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search\_result in INTEGER)
* 接口功能：给定用户id:user\_id，查找出关注他的人 的，从startFrom开始，长度为limitation的user信息列表，按照时间降序排序
* 返回值：同上
* 输入参数：
	* user\_id: INTEGER类型，被查找的人的id
	* startFrom: INTEGER类型，查找的起点
	* limitation: INTEGER类型，查找结果的长度
* 输出参数：
	* search\_result: sys_refcursor类型。用户的简单信息表格，包括用户的user\_id，用户昵称user\_nickname，用户的使用中头像avatar_id。table的属性为
	(user\_id, user\_nickname, avatar\_id)
* 已完成：否


### 18.FUNC\_ADD\_PRIVATE\_LETTER(sender\_user\_id in INTEGER, receiver\_user_id in INTEGER, content in VARCHAR2)
* 接口功能：给定发送者的id:sender\_user\_id，以及接受者id:receiver\_user\_id，给定内容content，添加一条私信消息。
* 返回值：同上
* 输入参数：
	* sender\_user\_id: INTEGER类型，发送者的id
	* receiver\_user\_id: INTEGER类型，接受者的id
	* content: VARCHAR2类型，私信的内容
* 输出参数：无
* 已完成：否


### 19.FUNC\_DELETE\_PRIVATE\_LETTER(private\_letter\_id)
* 接口功能：给定私信id,删除一条私信。
* 返回值：同上
* 输入参数：
	* private\_letter\_id
* 输出参数：无
* 已完成：否


### 20.FUNC\_QUERY\_PRIVATE\_LETTERS\_SEND\_TO\_USER(user\_id in INTEGER ,startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
* 接口功能：给定用户id:user\_id，查找出其他人给他发送的私信 的，从startFrom开始，长度为limitation的私信信息列表，按照时间降序排序
* 返回值：同上
* 输入参数：
	* user\_id: INTEGER类型，被查找的人的id
	* startFrom: INTEGER类型，查找的起点
	* limitation: INTEGER类型，查找结果的长度
* 输出参数：
	* search\_result: sys_refcursor类型。私信的信息表格，包括发送者的id:sender\_user\_id，私信的id: private\_letter\_id，私信的内容private\_letter\_content，私信的发送时间戳timestamp。table的属性为
	(sender\_user\_id, private\_letter, private\_letter\_content, timestamp)
* 已完成：否


### 21.FUNC\_QUERY\_MESSAGE\_IDS\_CONTAINS\_CERTAIN\_TOPIC\_ID(topic_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
* 接口功能：通过给定的topic\_id，以及起始索引startFrom，以及搜索长度limitation，获取包含该topic的推特message_id的列表。按照时间降序排序
* 返回值：同上
* 输入参数：
	* topic\_id: INTEGER类型，话题的id
	* startFrom: INTEGER类型，查找结果的起始索引
	* limitation: INTEGER类型，查找结果的长度
* 输出参数：
	* search\_result: sys_refcursor类型，推特id列表，table属性为(message\_id)
* 已完成：否

### 22.FUNC\_QUERY\_TOPIC\_IDS\_ORDER\_BY\_HEAT(startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
* 接口功能：获取从startFrom开始，长度为limitation的，按照话题topic的热度降序排序的话题id列表
* 返回值：同上
* 输入参数：
	* startFrom: INTEGER类型，查找结果的起始索引
	* limitation: INTEGER类型，查找结果的长度
* 输出参数：
	* search\_result: sys_refcursor类型，推特id列表，table属性为(topic\_id)
* 已完成：否


### 23.FUNC\_ADD\_LIKE(user\_id in INTEGER, message\_id in INTEGER)
* 接口功能：给定点赞者的id:user\_id，以及被点赞的推特id: message\_id，添加一条点赞信息。**同时，推特的被点赞总数需要+1,推特的热度需要+1，推特所拥有的所有话题的热度需要+1**。
* 返回值：同上
* 输入参数：
	* user\_id: INTEGER类型，点赞者的id
	* message\_id: INTEGER类型，推特的id
* 输出参数：无
* 已完成：否

### 24.FUNC\_DELETE\_LIKE(user\_id in INTEGER, message\_id in INTEGER)
* 接口功能：给定点赞者的id:user\_id，以及被点赞的推特id: message\_id，删除一条点赞信息。**同时，推特的被点赞总数需要-1,推特的热度需要-1，推特所拥有的所有话题的热度需要-1**。
* 返回值：同上
* 输入参数：
	* user\_id: INTEGER类型，点赞者的id
	* message\_id: INTEGER类型，推特的id
* 输出参数：无
* 已完成：否

### 25.FUNC\_QUERY\_MESSAGE\_IDS\_THAT\_USER\_LIKES(user\_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search\_result out sys\_refcursor)
* 接口功能：通过给定的user\_id，获取该用户点赞的所有推特 的，从startFrom开始，长度为limitation 的message\_id的列表。按照时间降序排序
* 返回值：同上
* 输入参数：
	* user\_id: INTEGER类型，用户的id
	* startFrom: INTEGER类型，查找结果的起始索引
	* limitation: INTEGER类型，查找结果的长度
* 输出参数：
	* search\_result: sys\_refcursor类型，推特id列表，table属性为(message\_id)
* 已完成：否


### 26.FUNC\_ADD\_COMMENT(user\_id in INTEGER, message\_id in INTEGER, content in VARCHAR2)
* 接口功能：给定发送者的id:user\_id，以及推特id:message\_id，给定内容content，添加一条评论消息。**同时推特的被评论数+1，推特的热度+1,推特包含的全部的热度+1**
* 返回值：同上
* 输入参数：
	* user\_id: INTEGER类型，发送者的id
	* message\_id: INTEGER类型，推特的id
	* content: VARCHAR2类型，评论的内容
* 输出参数：无
* 已完成：否


### 27.FUNC\_ADD\_COLLECTION(user\_id in INTEGER, message\_id in INTEGER)
* 接口功能：给定发送者的id:user\_id，以及推特id:message\_id，添加一条收藏。**同时推特的热度+1,推特包含的全部的热度+1**
* 返回值：同上
* 输入参数：
	* user\_id: INTEGER类型，发送者的id
	* message\_id: INTEGER类型，推特的id
* 输出参数：无
* 已完成：否


### 28.FUNC\_DELETE\_COLLECTION(user\_id in INTEGER, message\_id in INTEGER)
* 接口功能：给定发送者的id:user\_id，以及推特id:message\_id，删除一条收藏。**同时推特的热度-1,推特包含的全部的热度-1**
* 返回值：同上
* 输入参数：
	* user\_id: INTEGER类型，收藏者的id
	* message\_id: INTEGER类型，推特的id
* 输出参数：无
* 已完成：否


### 29.FUNC\_QUERY\_COLLECTIONS\_OF\_MINE(user\_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search\_result out sys\_refcursor)
* 接口功能：通过给定的user\_id，获取该用户收藏的所有推特 的，从startFrom开始，长度为limitation 的message\_id的列表。按照时间降序排序
* 返回值：同上
* 输入参数：
	* user\_id: INTEGER类型，用户的id
	* startFrom: INTEGER类型，查找结果的起始索引
	* limitation: INTEGER类型，查找结果的长度
* 输出参数：
	* search\_result: sys\_refcursor类型，推特id列表，table属性为(message\_id)
* 已完成：否


### 30.FUNC\_QUERY\_MESSAGE\_IDS\_THAT\_AT\_USER(user_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search\_result out sys\_refcursor))
 接口功能：通过给定的user\_id，获取艾特该用户的所有推特 的，从startFrom开始，长度为limitation 的message\_id的列表。按照时间降序排序
* 返回值：同上
* 输入参数：
	* user\_id: INTEGER类型，用户的id
	* startFrom: INTEGER类型，查找结果的起始索引
	* limitation: INTEGER类型，查找结果的长度
* 输出参数：
	* search\_result: sys\_refcursor类型，推特id列表，table属性为(message\_id)
* 已完成：否