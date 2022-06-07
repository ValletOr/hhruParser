import csv
import sys
import re
import requests
import pandas as pd

from time import sleep
from urllib.parse import quote_plus
from lxml import html

def ProfRoles(Roles):
    outMessage = ""
    for i in Roles:
        outMessage += "&professional_role=" + i
    return outMessage
    
def Salary(MinS):
    outMessage = ""
    if MinS != "-":
        outMessage = "&salary=" + MinS
    return outMessage

def Actuality(days):
    outMessage = "&search_period=" + days
    return outMessage

def UrlCreator ():
    '''
    --код_професии1;код_професии2;код_професии3...
    --мин_зарплата;макс_зарплата
    --актуальность(сколько дней назад выложено объявление. 0 - за всё время)
    '''
    url = "https://irkutsk.hh.ru/search/vacancy?area=35&only_with_salary=true&order_by=salary_asc&items_on_page=20&currency_code=RUR"
    url += ProfRoles(sys.argv[1].split(";"))
    url += Salary(sys.argv[2].split(";")[0])
    url += Actuality(sys.argv[3])
    return url

def Parsing ():
    titles = []
    salarys = []
    actualCount = 0
    useragent = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.41 Safari/537.36',}

    url = UrlCreator()
    print("Общий URL:" + url)
    pageSpec = requests.get(url, headers=useragent)
    if pageSpec.status_code != requests.codes.ok : raise Exception(f"http code == {pageSpec.status_code}")
    if not pageSpec.content or len(pageSpec.content) < 7: raise Exception(f"no content at {url}")
    elSpec = html.fromstring(pageSpec.content.decode('utf-8')).xpath("//h1")
    if elSpec:
        textSpec = elSpec[0].text_content()
        countSpec = re.sub(r'[^0-9]', '', textSpec)
        countSpec = 0 if len(countSpec) < 1 else int(countSpec)
    else :
        countSpec = 0
        
    pages=countSpec//20
    if(countSpec%20!=0): pages += 1

    for i in range(0, pages):
        urlP = url + "&page=" + str(i)

        page = requests.get(urlP, headers=useragent)

        if page.status_code != requests.codes.ok : raise Exception(f"http code == {page.status_code}")
        if not page.content or len(page.content) < 7: raise Exception(f"no content at {urlP}")
        
        for j in range(2, 30):
            xpath = "//*[@id=\"a11y-main-content\"]/div["+str(j)+"]/div/div[1]/div/div[1]/span"
            xpathToTitle = "//*[@id=\"a11y-main-content\"]/div["+str(j)+"]/div/div[1]/div[1]/div[1]/h3/span[1]/span/a"
            el = html.fromstring(page.content.decode('utf-8')).xpath(xpath)
            elTitle = html.fromstring(page.content.decode('utf-8')).xpath(xpathToTitle)
            if el and elTitle:
                counter_text = el[0].text_content().split('–')[0]
                count = re.sub(r'[^0-9]', '', counter_text)
                counter_text = elTitle[0].text_content()
                titles.append(counter_text)
                salarys.append(count)
                actualCount += 1
                
    
    print("Контрольный подсчёт: " + str(actualCount))
    d = {'name': titles,'salary': salarys}
    return d
print(sys.argv)
dictionary = Parsing()
df = pd.DataFrame(dictionary)
df.to_excel('./output.xlsx', index=False)
print(df)
