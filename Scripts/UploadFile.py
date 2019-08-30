import requests


filename='5cf475c53fc8b48789.jpg'
with open(filename,"rb") as f:
	file_content=f.read()

files={'file':(filename,file_content)}

r=requests.post("http://127.0.0.1:5000/api/file?token=fa47ad48e49ddd50e0d291a8ca466968",files=files)
print(r.ok)
print(r.text)