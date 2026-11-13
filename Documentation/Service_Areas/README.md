# C05–C08 配套用房 3D 初稿

状态：2026-11-13 已在 Unity 中生成、检查并保存，随本次场景提交保留。

## 布置

| 区域 | 本版内容 |
|---|---|
| C05 清洁／储物，净 2.60×3.10m | 浅木与灰绿金属耗材架、八个分类盒、封闭高工具柜、带真实盆腔的拖把池及壁装水龙头、挂墙拖把和扫帚、小清洁车。家具靠墙，中间保持操作空间。 |
| C06 电梯预留，净 2.80×3.10m | 北侧走廊可见的双扇关闭电梯门、深灰绿门套、楼层显示、呼梯按钮和 LIFT 标识。仅为静态入口外观，原井道空间保留，未加入轿厢、开门交互或跨层运行。 |
| C07 独立无障碍卫生间，净 2.60×3.10m | 东北侧凹盆坐便器与抬起的盖板、西北悬挂洗手盆及真实盆腔、镜子、皂液器、纸巾、固定扶手、处于上翻状态的转移扶手、紧急拉绳、手纸和小垃圾桶。原入口继续开放。 |
| C08 设备／弱电，净 2.80×3.10m | 东侧玻璃门网络机柜，含五层设备、端口、状态灯和短跳线；西墙弱电箱及规整高位线槽。保留设备前操作空间，未放办公桌或强电配电设备。 |

三间实际布置房间使用独立的浅灰地砖。构建器内部家具基准为 0.075m，集成时整体下移 0.074m，最终完成面为 **Y=0.001m**，仅比走廊结构板顶高 1mm。原 C05、C07、C08 下的分区占位面及地面文字随其纯占位根节点隐藏，原对象保留；墙体、开口及其他房间不变。

电梯入口位于原北侧开口内，关闭门碰撞位于 Z=3.335–3.395m；门套及按钮贴向走廊侧墙面。该门现在是预留井道的关闭边界，不再作为可穿行房间入口。既有结构、房间、中庭验证器仅在这个带生成标记的电梯门实际存在时识别此例外，并继续检查其余通行门。

## 卫生间几何参考

- 保留中心 X=7.97、Z=-1.90 的直径 1.525m 转身空间，实际物理检查采用完整外接方框，较仅检查圆周更保守。
- 坐便器中心 X=8.87，距东侧净墙 0.43m；座面距本房间完成面 0.440m。西侧预留 0.76×1.22m 无低位障碍的移位区域，上翻扶手不占此区域。
- 洗手盆朝东，投影 X=6.72–7.25、Z=-0.80–-0.20。盆下无落地柜；检查 0.76m 宽、0.30m 深、至完成面以上 0.685m 的膝部空间，排水管后退靠墙。
- 地面射线检查覆盖走廊、门洞和室内四个位置，未以角色的跨阶能力代替入口地面平整检查。

这些尺寸作为 Unity 方案的几何参考，来源为美国 Access Board 的 [转身、净地与膝部空间](https://www.access-board.gov/ada/chapter/ch03/)及[卫生间设施](https://www.access-board.gov/ada/chapter/ch06/)；拉绳设施表达也参考英国 [Approved Document M Volume 2](https://assets.publishing.service.gov.uk/media/66f6c5eec71e42688b65ee11/ADM__V2_with_2024_amendments.pdf)。本版不是完整建筑合规认证。

## 验证与查看

- Unity 脚本编译、场景保存、六张实际相机三维预览通过。最终控制台警告 0、错误 0。
- 实际编辑模式 CharacterController 走通 C05 各主要家具前方、C07 转身点／坐便器西侧／洗手盆前方、C08 机柜前方和 C06 门前，再返回走廊。
- 23 个可穿行门口、两条主走廊和两条庭院侧连廊保持畅通；电梯门作为刻意关闭的预留边界单独检查。
- 原八个教学／办公房间和 C04 中庭的实际编辑模式通行验证再次通过。
- 本轮未进入 Play，也未模拟真实轮椅、自动门或电梯运行。
- 场景：`Assets/Scenes/JEI_Floor01_Structure.unity`；独立新增组：`08_Service_Areas`。
- 菜单：`Tools/JEI/Service Areas` 下提供生成、验证、截图及各房间聚焦入口。
- 文件请求：`Library/JEI_Service.request` 可写入 `build`、`verify`、`capture`、`focus`、`focus_cleaner`、`focus_wc`、`focus_it` 或 `focus_lift`；结果见 `Library/JEI_Service.result`。
- 预览：`01_Service_Areas_3D.png`、`02_C05_Cleaner_Store.png`、`03_C07_Accessible_WC.png`、`04_C08_IT_Services.png`、`05_C06_Lift_Entrance.png`、`06_C08_Network_Rack.png`；全层总览也已更新。

## 本地保留

- 改动前基线为 `28ed0a2f3bcff4a2c685bb09ea7e4fa0021eb5e6`。
- 原完整场景、全层预览及资源哈希保存在 `Library/JEI_Service_Baseline/`，每次生成前还备份至 `Library/JEI_Service_Backups/`。
- 生成结果 `Library/JEI_Service_Build.result`；中庭与房间兼容检查分别为 `Library/JEI_Service_Courtyard_Compatibility.result`、`Library/JEI_Service_Interiors_Compatibility.result`；范围核验 `Library/JEI_Service_Scope_Check.json`。Library 文件不入仓库。
- 范围核验：24,653 个原场景对象全部保留，新增 2,522 个对象均位于 `08_Service_Areas` 下；24,647 个旧对象内容完全不变。旧楼层根只追加一个子引用，三个纯占位根仅切换为隐藏，另有下述两个灯光归一化字段。原 831 个家具文件 SHA256 全同，78 个新增资源与 `.meta` 配对完整，三个新脚本及 21 个 C07 自建网格引用均解析通过。
- Unity 同时将原 C04 两盏 range=4m 壁灯的阴影近裁面由 0.03m 归一化为 0.04m；与[官方灯光文档](https://docs.unity.cn/Manual/class-Light.html)规定的 1% range 下界一致。本地 URP 编辑器源码也确认此下界；灯位、颜色、强度、范围及其余中庭对象未改。此自动调整单独列入范围报告。
- 既有 `ProjectSettings/ShaderGraphSettings.asset` 自动设置变更保持原样，未处理。
