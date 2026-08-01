//@ts-ignore
import * as fs from "fs"
import type {TI18nKv} from "./keys"
export type {TI18nKv}

function writeFile(path:string, content:string){
	fs.writeFileSync(path, content, {encoding: "utf8"})
}

export class I18nForOne{
	lang=""
	descr=""
	forZone=[] as string[]
	kv?:TI18nKv
	static mk(lang:string, kv:TI18nKv){
		const z = new I18nForOne()
		z.lang = lang
		z.kv = kv
		return z
	}
}

export class I18nMgr{
	static _inst = new I18nMgr()
	static get inst(){
		return I18nMgr._inst
	}
	lang_i18nForOne:Map<string,I18nForOne> = new Map()
	register(forOne:I18nForOne){
		const z = this
		const existing = z.lang_i18nForOne.get(forOne.lang)
		if(existing != void 0){
			throw new Error(`I18n for ${forOne.lang} already registered.`)
		}
		z.lang_i18nForOne.set(forOne.lang, forOne)
	}
	writeJson(path:string){
		const z = this
		for(const [lang,forOne] of z.lang_i18nForOne){
			const json = JSON.stringify(forOne.kv, null, 2)
			writeFile(`${path}/${lang}.json`, json)
		}
	}
}

