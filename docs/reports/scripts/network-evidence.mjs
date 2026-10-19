const { chromium } = await import(process.env.PLAYWRIGHT_MODULE || 'playwright-core')
import { mockCreateWasteRequest } from '../../../frontend/src/api/mockWasteRequests.js'
const browser=await chromium.launch({executablePath:process.env.CHROMIUM_PATH || '/usr/bin/chromium',headless:true,args:['--no-sandbox','--disable-dev-shm-usage','--window-size=1440,1000','--window-position=0,0','--remote-debugging-port=9222','--remote-debugging-address=127.0.0.1','--remote-allow-origins=http://127.0.0.1:9222']})
const context=await browser.newContext({viewport:{width:1440,height:900}})
await context.route('http://127.0.0.1:59999/api/waste-requests',async route=>{
 if(route.request().method()==='OPTIONS'){await route.fulfill({status:204,headers:{'Access-Control-Allow-Origin':'*','Access-Control-Allow-Methods':'GET, POST, OPTIONS','Access-Control-Allow-Headers':'Content-Type'}});return}
 const response=await mockCreateWasteRequest(route.request().postDataJSON())
 await route.fulfill({status:response.status,contentType:'application/json',headers:{'Access-Control-Allow-Origin':'*'},body:JSON.stringify(response.data)})
})
const app=await context.newPage()
await app.goto('http://127.0.0.1:5176/new')
const session=await context.newCDPSession(app)
const {targetInfo}=await session.send('Target.getTargetInfo')
const devtools=await context.newPage()
await devtools.goto(`http://127.0.0.1:9222/devtools/inspector.html?ws=127.0.0.1:9222/devtools/page/${targetInfo.targetId}&panel=network`)
await devtools.getByRole('button',{name:'Toggle screencast',exact:true}).click({timeout:3000}).catch(()=>{})
await devtools.getByText('Fetch/XHR',{exact:true}).click()
await app.evaluate(async()=>{
 const api=await import('/src/api/wasteRequests.js')
 const body={title:'Вывоз отработанных кислот',laboratoryId:3,wasteTypeId:7,description:'Накопилось 20 литров отработанной кислоты'}
 const created=await api.createWasteRequest(body)
 if(created.status!=='New')throw new Error('Expected New')
 try {await api.createWasteRequest({...body,title:'abc'})} catch(e){if(e.status!==400)throw e;return}
 throw new Error('Expected HTTP 400')
})
await devtools.waitForTimeout(500)
console.log('tabs',await devtools.getByRole('tab').allTextContents())
console.log((await devtools.locator('body').innerText()).slice(0,1800))
await devtools.screenshot({path:new URL('../images/lab-06-network.png',import.meta.url).pathname,fullPage:true})
await browser.close()
