import requests
import json
import hashlib

headers = {'Content-type': 'application/json'}
SALT="BSDN"
user_data=dict(
	email="test1@example.com",
	nickname="Amy"
)
password="example_password"
passwordHash=hashlib.md5("{}{}".format(password,SALT).encode())
user_data["passwordHash"]=str(passwordHash.digest().hex())
#print(passwordHash.digest().hex())

article_data=dict(
	title="Hello, World 5",
	content="This is the 5th article of this system\n一个含有中文的博文"
)
print(json.dumps(article_data))

r=requests.post("http://127.0.0.1:5000/api/session",headers=headers,data=json.dumps(user_data))
print(r.text)
print(r.status_code)


rjson=r.json()
token=rjson['data']['sessionToken']
userId=rjson['data']['sessionUserId']

#r=requests.post("http://127.0.0.1:5000/api/article?token={}".format(token),headers=headers,data=json.dumps(article_data))
#print(r.text)
#print(r.ok)

r=requests.delete("http://127.0.0.1:5000/api/article?id={}&token={}".format(109,token))
print(r.text)
print(r.ok)