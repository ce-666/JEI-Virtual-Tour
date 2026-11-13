# C02、C03 公共卫生间 3D 初稿

2026-11-13：已在 Unity 中生成、验证并保存，随本次场景提交保留。按用户最新要求，本轮不继续制作 C06 电梯井，也不重建上一轮的东侧配套用房。

## 本版布置

| 区域 | 净尺寸 | 布置 |
|---|---|---|
| C02 女卫生间 | 5.60×3.10m | 西侧三间厕位，东侧两只壁挂洗手盆、镜子、皂液器、纸巾和小垃圾桶。入口保留在北侧。 |
| C03 男卫生间 | 5.60×3.10m | 西侧两间厕位，北侧两只壁挂洗手盆；东侧两只小便器，中间设隔板，其中一只安装较低。入口保留在南侧，增加短挡板遮住小便器视线。 |

两间延续浅灰地砖、奶油白洁具、灰绿色隔板及金属五金。坐便器、洗手盆和小便器均为实际三维凹盆网格；隔间补齐门框、门板、锁扣、把手、使用指示、挂钩与手纸。

厕位净宽约 1.00m、净深约 1.78m，门扇宽 0.78m。本版五扇厕位门均固定展示为向内打开 90°，便于检查内部，尚未加入开关门交互。地砖完成面为 Y=0.001m，与原走廊板顶只差 1mm。

## 设计参考

入口视线遮挡、小便器之间独立隔板、设置一只低位小便器，以及隔间尺寸，参考英国政府 [Approved Document T 2024（2025 修订）](https://assets.publishing.service.gov.uk/media/6901feb971b575684c3cf7c4/ADT_2024_rev16Jun25.pdf) 第 1.22、2.2、2.4 节及表 6.1 的相关原则。这些是本次 Unity 布置的几何参考；未根据全楼核定人数计算器具数量，也不代表完整建筑合规认证。

## 已完成的检查

- Unity 编辑器脚本编译通过，场景已保存；预览来自实际场景相机渲染。最终控制台警告 0、错误 0。
- 使用实际编辑模式 CharacterController（高 1.80m、半径 0.30m）依次进入五间敞开厕位，到达四只洗手盆和两只小便器前方，并返回走廊。
- 沿两个入口对走廊、门洞及室内地面做射线检查，未出现地砖台阶。
- 从男厕门口两侧向两只小便器分别做视线检测，均先击中入口挡板。
- 23 个可穿行门口、两条 2.40m 主走廊和两条中庭侧连廊未被堵塞；原关闭电梯预留入口沿用既有验证规则。
- 本轮家具碰撞均在各自房间范围内，Mesh 与 Material 引用均可解析。
- 本轮未进入 Play；检查范围是静态三维布置和编辑模式碰撞、通行。

## 在 Unity 中查看

场景：`Assets/Scenes/JEI_Floor01_Structure.unity`。

新增物体全部在楼层的 `09_Public_Restrooms` 下，分为 `C02__Womens_restroom` 与 `C03__Mens_restroom`。原 C02、C03 纯占位分区根节点仅停用，原物体保留。使用 `Tools/JEI/Public Restrooms` 菜单可聚焦、验证或截图；生成菜单会只重建本组带生成标记的内容，请先保存手工布置。

文件请求入口为 `Library/JEI_Restrooms.request`，支持 `build`、`verify`、`capture`、`focus`、`focus_women`、`focus_men`；结果位于 `Library/JEI_Restrooms.result`。

预览：

- `01_Restrooms_3D_Overview.png`：两个房间与周围走廊。
- `02_C02_Womens_Restroom.png`：女厕入口视角。
- `03_C03_Mens_Restroom.png`：男厕入口及隐私挡板。
- `04_C02_Cubicles.png`：女厕三间厕位。
- `05_C03_Urinals_and_Washbasins.png`：男厕小便器及隔板近景。
- `06_C02_Room_Layout.png`：女厕三维俯视。

全层预览 `Documentation/Floor01_Furnishings/01_Overview.png` 同步更新。

## 本地备份与范围

本轮基线在 `Library/JEI_Public_Restrooms_Baseline/`，包含原完整场景、总览和旧资源哈希；构建前场景另备份至 `Library/JEI_Restroom_Backups/`。成功构建记录为 `Library/JEI_Restrooms_Build.result`，最终场景范围审计为 `Library/JEI_Public_Restrooms_Scope_Check.json`；Library 文件不入仓库。

基线的 27,175 个旧场景对象全部保留，其中 27,172 个字节完全相同。仅楼层根追加 `09` 子引用、C02 与 C03 原纯占位根切换为停用；新增 2,712 个对象均位于 `09_Public_Restrooms`。原教学办公房间、中庭、东侧配套用房、电梯预留物体、RenderSettings 与原太阳灯内容均未改变。

987 个旧家具文件中 986 个 SHA256 相同。C08 的玻璃材质仅 `_SrcBlend` 由 5 自动归一化为 1：将该行逆替换后，文件哈希与基线完全一致。其既有 `_BlendModePreserveSpecular=1` 和 `_ALPHAPREMULTIPLY_ON` 对应本地 URP `BaseShaderGUI.cs` 第 1098–1114 行计算出的 `BlendMode.One`；保留引擎校正，颜色、透明度、纹理与其他字段均未改。详细哈希和本地源码路径见范围审计报告。

既有 `ProjectSettings/ShaderGraphSettings.asset` 自动设置变更保持原样，未处理。
