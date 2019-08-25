import requests
import json
import hashlib

headers = {'Content-type': 'application/json'}
SALT="BSDN"

user_data=dict(
	email="test1@example.com",
	nickname="Sim"
)
password="example_password"
passwordHash=hashlib.md5("{}{}".format(password,SALT).encode())
user_data["passwordHash"]=str(passwordHash.digest().hex())
#print(passwordHash.digest().hex())

article_data=dict(
	title="Hello, World 4",
	content="This is the 4th article of this system\nWith a \\n signature\nAnd a markdown image url ![Google Logo](\"https://www.google.co.jp/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png\")"
)


r=requests.post("http://127.0.0.1:5000/api/session",headers=headers,data=json.dumps(user_data))
print(r.text)
print(r.status_code)


rjson=r.json()
token=rjson['data']['sessionToken']
userId=rjson['data']['sessionUserId']

r=requests.post("http://127.0.0.1:5000/api/article?token={}".format(token),headers=headers,data=json.dumps(article_data))
print(r.text)
print(r.ok)