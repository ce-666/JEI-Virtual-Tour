# N04 小组项目工作室

状态：2026-11-11 小组项目工作室已完成用户检查确认，纳入本次提交及个人 fork 推送版本。

## 本轮布置

- 低背 L 形灰绿软包卡座，共 3 个固定坐位，坐高 0.47m、背高 1.10m；两段卡座在转角接合，保留连续背垫及木框。
- 两张 1.19×1.50m 轻便拼接桌，总工作面 2.40×1.50m、高 0.75m，带中缝、独立脚轮及桌下连接件。另配 3 把米白／灰绿活动椅，形成 6 人围坐协作区。
- 桌面为小组共用的校园图纸、低矮场地模型、一台共用笔记本电脑和少量笔／材料卡，撤除了原先按个人分配的书本和阅读灯。
- 西墙 2.70×1.40m 协作白板，包含观察、构思、制作流程及便签，方便成员起身画方案。
- 东窗下 3.40×0.60m 项目操作台，高 0.86m，保留窗台采光；两端材料抽屉，中间工作面，摆放正在制作的纸板模型、切割垫及材料托盘。
- 南墙右侧 6 个组员储物格及封闭材料抽屉，上方为 TO DO／MAKING／REVIEW 三列项目进度板；左侧为小组主题／成员板与移动资料柜。
- 原阅读扶手椅、椭圆茶几和两组高书柜已撤除。外墙、门窗、楼梯和其他房间保持原样。

## 检查结果

- Unity 实际脚本编译、N04 局部生成、场景保存与六个视角的真实三维渲染通过；最终实际查看控制台为错误 0、警告 0。
- 在编辑模式使用实际 CharacterController 复走：入口至南／东开放活动区、三把活动椅侧方、卡座两端、白板书写前、操作台三段、六格储物及资料柜前均可到达。
- 卡座与桌面之间的膝部空间用于坐下使用；通行验证从卡座两端接近，不将该窄缝作为步行通道。入口路线在到达南侧椅子前转向开放区。
- 24 个既有门口、两条 2.40m 公共走廊，以及 N01—N03、S01—S04 原有通路的检查通过。
- 3 个卡座席、3 把活动椅、2 张拼接模块桌、6 个组员格和所有主要配套的数量／朝向检查通过；家具碰撞体位于房内，操作台低于窗台。
- 与本轮改动前场景比较：旧 N04 子树以外的 21,224 个对象全部保留，21,223 个内容完全相同；唯一变化为室内布置根的子节点从旧 N04 切换为新 N04。583 个既有家具资源内容完全相同。
- 已检查六个新预览的实际画面，讨论墙文字作了局部对比度调整；共享字体材质和其他房间材质未修改。
- 此前 Unity 曾在旧阅读室 Play 检查结束后发生原生音频／窗口清理崩溃。本轮未重复 Play，当前结论为编译、编辑模式物理通行与渲染验证；不将旧阅读室的 Play 报告当作新工作室测试结果。
- 此前已有的 `ProjectSettings/ShaderGraphSettings.asset` 自动设置改动仍保留，未处理。

## 查看和继续修改

- 场景：`Assets/Scenes/JEI_Floor01_Structure.unity`，Unity 当前保留 N04 的三维场景视角。
- 当前预览：`Floor01_Furnishings/45_N04_Project_Studio_Cutaway.png`、`46_N04_Team_Booth.png`、`47_N04_Discussion_Wall.png`、`48_N04_Window_Workbench.png`、`49_N04_Studio_From_Entry.png`、`51_N04_Team_Storage_and_Progress.png`。
- `39`—`44` 号 N04 预览属于已替换的阅读室试稿；`50_N04_Runtime_Project_Studio.png` 仅由未来 Play 验证生成，本轮没有生成该图。
- 总览快捷键 **8** 的标签已更新为项目工作室；当前未启动 Play。
- 构建器沿用 `Assets/Editor/JEIReadingSuite.cs` 及其原 `.meta` GUID；局部生成菜单为 `Tools/JEI/Interiors/Update N04 Project Studio Only`，文件请求仍为 `build_n04`。
- 内部房间组沿用 `N04_Reading_and_Seminar`，以便安全替换旧试稿；房间门牌及对外描述已更新为 TEAM PROJECT STUDIO。

## 本地备份与后续状态

- 改动前场景、脚本、旧预览及资源哈希：`Library/JEI_N04_Project_Studio_Baseline/`。
- 本轮生成／验证记录：`Library/JEI_N04_Project_Studio_Build.result`；范围核验：`Library/JEI_N04_Project_Studio_Scope_Check.json`。
- 最早的空房基线和阅读室生成记录仍在 `Library/JEI_N04_Baseline.unity`、`Library/JEI_N04_Build.result`；旧阅读室实际漫游结果为 `Library/JEI_N04_Runtime_Before_Crash.result`。
- 旧事故报告为系统的 `Unity-2026-11-11-135128.ips`：原生主线程 EXC_BAD_ACCESS／SIGSEGV，调用链包含音频更新、错误报告／退出和 Metal 窗口销毁；首次音频错误的原因尚未确定。没有修改 Unity 版本或音频配置。
- 以上 Library 备份不入仓库。本次推送目标仅为个人 fork `ce-666/JEI-Virtual-Tour`，不向队友仓库推送。
