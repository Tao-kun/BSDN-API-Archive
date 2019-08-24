import requests
import json
import hashlib

headers = {'Content-type': 'application/json'}
SALT="BSDN"

user_data=dict(
	email="test@example.com",
	nickname="Tom"
)

password="example_password"
passwordHash=hashlib.md5("{}{}".format(password,SALT).encode())
#print(passwordHash.digest().hex())

user_data["passwordHash"]=str(passwordHash.digest().hex())

#print(json.dumps(user_data))

# 注册
r=requests.post("http://127.0.0.1:5000/api/user",headers=headers,data=json.dumps(user_data))
print(r.text)
print(r.status_code)

# 登录
r=requests.post("http://127.0.0.1:5000/api/session",headers=headers,data=json.dumps(user_data))
print(r.text)
print(r.status_code)

rjson=r.json()
token=rjson['data']['sessionToken']
userId=rjson['data']['sessionUserId']

# 退出
#r=requests.delete("http://127.0.0.1:5000/api/session?token={}".format(token))
#print(r.text)
#print(r.status_code)

# 注销

#r=requests.delete("http://127.0.0.1:5000/api/user/{}?token={}".format(userId,token))
#print(r.text)
#print(r.status_code)