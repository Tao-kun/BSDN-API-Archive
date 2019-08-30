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

r=requests.post("http://127.0.0.1:5000/api/follow/52?token=fa47ad48e49ddd50e0d291a8ca466968")
print(r.text)
print(r.ok)