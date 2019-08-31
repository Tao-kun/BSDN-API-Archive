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

tagNameList=["C","C++","C#","Java"]


r=requests.post("http://127.0.0.1:5000/api/session",headers=headers,data=json.dumps(user_data))
print(r.text)
print(r.status_code)


rjson=r.json()
token=rjson['data']['sessionToken']
userId=rjson['data']['sessionUserId']


for tagName in tagNameList:
	tag_data=dict(tagName=tagName)
	r=requests.post("http://127.0.0.1:5000/api/tag?token={}".format(token),headers=headers,data=json.dumps(tag_data))
	print(r.text)
	print(r.ok)