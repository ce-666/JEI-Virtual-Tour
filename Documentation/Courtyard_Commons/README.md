# C04 中庭绿化、弧形长凳、学生作品与暖光初稿

状态：2026-11-11 已生成、检查并保存至现有 Unity 场景，用户已要求提交并推送。推送目标仅为用户自己的 fork `ce-666/JEI-Virtual-Tour`（`origin`）。

## 布置

- 保留中庭原有约 7.80×6.40m 净空间与南侧 1.60m 门口。绿化岛中心偏向西侧，坐标为 X=-1.15、Z=0.55。
- 一处半径 0.90m、离成品地面高 0.35m 的浅石材花池，配分叉小树、少量低矮绿植和自然石。树最高世界坐标 Y=2.935m，树冠与现有墙体保持距离。
- 两段面朝外的弧形木长凳，内／外半径 1.10／1.65m，圆心角分别为 25°—100°、155°—230°。座面采用分开的曲木条，配轻巧金属脚和三条木靠背。
- 长凳座面离成品地面高 0.47m，靠背顶部离地 0.95m。两段之间保留开口，东侧和入口前留出较大开放区域。
- 新增 7.78×6.38m 浅暖灰石材铺装及细缝，成品面为 Y=0.075m；薄地面碰撞体让人物实际站在该高度。原结构楼板、分区面及标签均保留，铺装覆盖原绿色占位效果。
- 东侧新增一处学生作品展台，中心 X=3.25、Z=1.35，朝西面向庭院。奶油色石材台面世界高度 Y=0.725m，上方为暖陶色与灰绿色相交的中空椭圆环，配支点和作品铭牌；整件作品顶部约 Y=1.687m。
- 东北、东南角各新增一只 1.65×0.50m 低花槽，中心分别为 (X=2.85, Z=2.80) 和 (X=2.85, Z=-2.80)。采用奶油色槽体、细长弯曲观赏草及少量穗和地被，整体保持在 Y=0.86m 以下，低于现有 Y=0.90m 窗台。

## 暖色照明

- 新增独立 `C04_Warm_Lighting` 组：两盏北墙暖白壁灯、两段凳下隐藏弧形 LED 和一个台面嵌入式作品上射灯。均为真实实时 Spot 光源，色温 3000K，配柔和阴影与发光灯罩／灯带材质；灯具无碰撞体。
- 两壁灯位于北窗两端的实体墙墩，灯体中心为 (X=±3.55, Y=2.12, Z=3.09)，朝下并向庭院内部照射，避免遮窗。凳下灯条低于座面，不伸出长凳轮廓。
- 壁灯 range=4.0、intensity=2.0；凳下灯 range=1.6、intensity=0.16；展台灯 range=1.3、intensity=1.0。凳下光强在实际渲染后调低，避免出现过亮光斑。
- 为新灯单独指定 URP Medium 阴影分辨率层级，复用现有图集；未更改渲染管线或质量资源。
- `07_Warm_Lighting_Evening.png`、`08_Artwork_Lighting_Evening.png` 是真实相机的临时黄昏光照预览。截图函数在 `finally` 中恢复环境光及全部方向光强度，保存的场景仍使用原有白天照明；未新增昼夜切换功能。

## 验证

- Unity 实际脚本编译、局部生成、场景保存和八张真实三维预览渲染通过；实际查看控制台为错误 0、警告 0。
- 在编辑模式使用实际 CharacterController 从公共走廊穿过中庭原南门，走完整个座椅外侧环路、到达东侧开放区域，再返回公共走廊。
- 环路半径为 2.12m，每 2.5°采样，覆盖两段长凳外侧的接近位置。花池与凳背之间的绿化间隙不作为通行路线。
- 24 个既有门口、两条 2.40m 主走廊和两条 2.40m 庭院侧连廊均无阻挡；新增碰撞体均位于中庭内。
- 长凳碰撞沿实际弧线分成每段 8 个小盒，花池采用低圆柱凸 MeshCollider，避免用大矩形填死环形通路。
- 树冠高度和墙边范围、所有新增网格／材质引用检查通过；50 个新增资源的 `.meta` 与场景 GUID 引用完整。
- 改动前 23,047 个场景对象全部保留，其中 23,046 个内容完全相同，唯一变化为楼层根新增 `07_Courtyard_Commons` 子节点；增加 843 个对象。663 个既有家具资源内容全部不变。
- 作品及角落花槽追加检查：实际 CharacterController 可到达展台观看点及两个花槽前方，继续返回公共走廊；原有入口和完整座椅外环通行检查仍通过。展台整体边界、花槽绿植高度及网格／材质引用全部通过。
- 追加前的 23,890 个对象全部保留，23,889 个内容完全相同；唯一旧对象变化为 C04 根 Transform 追加 details 子节点。新增 610 个对象，763 个既有家具资源 SHA256 不变，22 个新增资源及其 `.meta` 配对完整，新增场景 GUID 均可解析。
- 五个 3000K 实时光源、发光材质、灯具边界和零新增碰撞体检查通过；全套实际 CharacterController 通行检查再次通过。最终控制台警告 0、错误 0。
- 暖光追加前 24,500 个对象全部保留，其中 24,499 个字节相同；唯一旧对象变化为 C04 根追加暖光子节点。新增 153 个对象；807 个既有资源不变，12 个新资源及 `.meta` 完整。原 RenderSettings 和方向光字节完全一致，临时黄昏设置未落盘。
- 本轮为编辑模式几何、碰撞与渲染验证，没有重复启动 Play；此前的 Unity 原生音频／退出崩溃不在本轮修复范围内。

## 查看与后续修改

- 场景：`Assets/Scenes/JEI_Floor01_Structure.unity`。
- 编辑器聚焦：`Tools/JEI/Courtyard/Focus Courtyard`；现有总览控制器新增快捷键 **9** 聚焦中庭。
- 局部生成：`Tools/JEI/Courtyard/Apply Green Island and Curved Benches`；仅替换带生成标记的 `07_Courtyard_Commons`，与 `06_Approved_Interiors` 的八个房间分开管理。手动加入的对象会被保留。
- 仅更新学生作品和两角花槽：`Tools/JEI/Courtyard/Add Student Artwork and Corner Planting`，只替换带生成标记的 `C04_Art_and_Corner_Planting` 子组，保留已有中庭树木、长凳和铺地。
- 仅更新中庭暖光：`Tools/JEI/Courtyard/Add Warm Courtyard Lighting`，只替换带生成标记的 `C04_Warm_Lighting` 子组。
- 构建器：`Assets/Editor/JEICourtyardCommons.cs`；保存、检查与截图工具：`Assets/Editor/JEICourtyardTools.cs`。
- 文件请求：`Library/JEI_Courtyard.request` 可写入 `build`、`details`、`lighting`、`verify`、`capture` 或 `focus`；结果在 `Library/JEI_Courtyard.result`。
- 六张白天预览为本目录的 `01_Courtyard_3D_Overview.png`、`02_From_Entrance.png`、`03_Curved_Benches.png`、`04_East_Open_Space.png`、`05_Student_Artwork.png`、`06_Low_Corner_Planting.png`；全层总览 `Documentation/Floor01_Furnishings/01_Overview.png` 也已更新。
- 若重新生成整层结构，需要重新应用房间布置及本中庭布置。

## 本地备份

- 本轮基线提交为 `031db236ecc73a0dcb31d940871d4c332d3b587f`。
- 改动前完整场景、脚本、总览及资源哈希：`Library/JEI_Courtyard_Baseline/`；每次生成前还会备份到 `Library/JEI_Courtyard_Backups/`。
- 生成与检查结果：`Library/JEI_Courtyard_Build.result`；范围核验：`Library/JEI_Courtyard_Scope_Check.json`。以上 Library 文件不入仓库。
- 本次作品及花槽追加前的完整场景、脚本、原预览与资源哈希：`Library/JEI_Courtyard_Details_Baseline/`；生成结果：`Library/JEI_Courtyard_Details_Build.result`；范围核验：`Library/JEI_Courtyard_Details_Scope_Check.json`。以上 Library 文件不入仓库。
- 暖光加入前的完整场景、原预览与资源哈希：`Library/JEI_Courtyard_Lighting_Baseline/`；最终生成结果：`Library/JEI_Courtyard_Lighting_Build.result`；范围核验：`Library/JEI_Courtyard_Lighting_Scope_Check.json`。以上 Library 文件不入仓库。
- 既有 `ProjectSettings/ShaderGraphSettings.asset` 自动设置变更继续保留，未处理。
