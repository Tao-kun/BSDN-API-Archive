import hashlib
import json

import requests

headers = {'Content-type': 'application/json'}
SALT = "BSDN"
user_data=dict(
    email="test1@example.com",
    nickname="Amy"
)
password = "example_password"
passwordHash = hashlib.md5("{}{}".format(password, SALT).encode())
user_data["passwordHash"] = str(passwordHash.digest().hex())
# print(passwordHash.digest().hex())


r = requests.post("http://127.0.0.1:5000/api/session", headers=headers, data=json.dumps(user_data))
print(r.text)
print(r.status_code)

rjson = r.json()
token = rjson['data']['sessionToken']
userId = rjson['data']['sessionUserId']

at_list = [(1, 1), (1,2), (1, 3)]

# add
for t in at_list:
    url = "http://127.0.0.1:5000/api/article/{}/tag/{}?token={}".format(*t, token)
    print(url)
    r = requests.post(url)
    print(r.text)
    print(r.status_code)


# delete
for t in at_list:
    url = "http://127.0.0.1:5000/api/article/{}/tag/{}?token={}".format(*t, token)
    r=requests.delete(url)
    print(r.text)
    print(r.status_code)
