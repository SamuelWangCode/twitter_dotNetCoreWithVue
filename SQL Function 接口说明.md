# SQL Function 接口说明

此文档由c#小组（杨紫超，周宇东，魏敬杰）负责编写。
SQL小组通过阅读此文档，编写相应的SQL脚本。
SQL脚本文件统一放置在根目录下的SQLs文件夹中。

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
	> * 对于输出参数为数据表的函数，输出参数类型应为sys_refcursor

* 异常处理

	> * SQL函数编写者应在定义函数内部时对可能出现的错误进行处理，尽量避免应函数执行错误而抛出异常
	> * SQL函数编写者应以状态码的值表示异常状态

* 注意事项

	> * 若使用select into语句来对单值输出参数赋值时应考虑到检索结果为空或多项结果的情况
	> * 小组成员可自行将编写过程中踩过的坑添加到注意事项中

* 接口样例

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
	> * 该代码块定义了一个根据热度heat来获取TOPIC表的函数
	> * 使用open for语句将查询结果装载入sys_refcursor类型的输出参数中
	> * 当没有查询结果时返回0
	> * 查询成功时返回1

## 需求接口
### FUNC\_CHECK\_USER\_EMAIL\_EXIST(email in VARCHAR)
* 接口功能：检查用户Email是否存在于数据库中
* 返回值：用户Email存在于数据库时返回1，不存在时返回0
* 输入参数：

	* email：VARCHAR类型，存放待检查的Email

* 输出参数：无

### FUNC\_CHECK\_USER\_NICKNAME\_EXIST(nickname in VARCHAR)
* 接口功能：检查用户昵称是否存在于数据库中
* 返回值：用户昵称存在于数据库时返回1，不存在时返回0
* 输入参数：

	* nickname：VARCHAR类型，存放待检查的用户昵称

* 输出参数：无

### FUNC\_USER\_SIGN\_UP(email in VARCHAR, nickname in VARCHAR, password in VARCHAR)
* 接口功能：通过给定的用户信息向数据库添加新用户
* 返回值：注册成功返回1，失败返回0
* 输入参数：
	* email：VARCHAR类型，存放用户Email
	* nickname：VARCHAR类型，存放用户昵称
	* password：VARCHAR类型，存放用户密码

* 输出参数：无

		
