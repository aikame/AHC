from django.http import HttpResponse
from django.shortcuts import render

def home(request):
    return render(
        request,
        'main_page/index.html',
    )

def settings(request):
    return render(
        request,
        'settings/index.html',
    )
def employee(request):
    return render(
        request,
        'employee/index.html',
    )
def computer(request):
    return render(
        request,
        'computer/index.html',
    )