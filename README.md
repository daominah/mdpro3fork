## MDPro3

A new version of YGOPro in Unity with MasterDuel Assets.

Unity version: 6000.0.10f1

### Other required folders

* Platforms/Android: You can find it from the Android apk.

* Platforms/StandaloneWindows64: You can find it from the released MDPro3(windows).

* Platforms/iOS: You can find it from the released MDPro3(iOS).

* Platforms/Mac: You can find it from the released MDPro3(Mac).

* Picture/Art: https://code.mycard.moe/mycard/hd-arts

* Picture/Closeup: https://code.mycard.moe/mycard/ygopro2-closeup

* Picture/DIY: You can find it from the released MDPro3

* Sound: You can find it from the released MDPro3

### Tools

* DumpShaders: Used for replacement during built-in assets packaging.

* YGO Classes: Used to compile dependencies from YGOPro in this project.

* Translations: python scripts used to split translation.csv to translation.conf.

* QuickBMS: decrypt IDS_ITEM.bytes(names of items in Master Duel) and IDS_ITEMDESC.bytes(descriptions of items in Master Duel);

### For Contributors:

* If you want to edit in-game translations, please edit Tools/Translations/translations.csv with Excel. Do not edit translation.conf.

* If you want to edit bot.conf(Windbot), please edit it on YGOMobile(https://github.com/fallenstardust/YGOMobile-cn-ko-en), this project copy these files from it.

### Unity Notes:
Unity 6000.32f1 Crashes when loading the following Assetbundles:
fxp_HL_EXdeck_001 (Material 2020.3.48f1)
PlayableGuide_C001_Far
PlayableGuide_C001_Far_Mat13
PlayableGuide_C001_Near
PlayableGuide_C001_Near_Mat13
