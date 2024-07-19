from django.conf import settings
from django.conf.urls.static import static

def upload_emp_avatar(f):
    with open("some/file/name.txt", "wb+") as destination:
        for chunk in f.chunks():
            destination.write(static.chunk)