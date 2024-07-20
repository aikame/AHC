from django import forms
class UploadFileForm(forms.Form):
    avatar = forms.ImageField()