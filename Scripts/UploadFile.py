import requests
import hashlib

filename=r'5cf475c53fc8b48789.jpg'
with open(filename,"rb") as f:
	file_content=f.read()

md5=hashlib.md5(file_content).hexdigest()
print(md5)

files={'file':(filename,file_content)}

r=requests.post("http://114.115.128.109/api/file?token=83f6df4726f552a67605404239f9f1cd",files=files)
print(r.ok)
print(r.text)