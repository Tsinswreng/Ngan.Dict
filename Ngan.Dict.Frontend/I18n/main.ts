import { I18nForOne, I18nMgr, TI18nKv } from "./i18n"

function R(lang:string, kv:TI18nKv){
	return I18nMgr.inst.register(I18nForOne.mk(lang, kv))
}

import en from "./langs/en"
R("en", en)

import zh_TW from "./langs/zh-TW"
R("zh-TW", zh_TW)

import zh_CN from "./langs/zh-CN"
R("zh-CN", zh_CN)

import ja from "./langs/ja"
R("ja", ja)

import fr from "./langs/fr"
R("fr", fr)

import it from "./langs/it"
R("it", it)

import es from "./langs/es"
R("es", es)

import pt from "./langs/pt"
R("pt", pt)

import de from "./langs/de"
R("de", de)

import ko from "./langs/ko"
R("ko", ko)

import ru from "./langs/ru"
R("ru", ru)

import th from "./langs/th"
R("th", th)

import vi from "./langs/vi"
R("vi", vi)

let firstArg:string
//@ts-ignore
firstArg = process.argv[2]
I18nMgr.inst.writeJson(firstArg)
