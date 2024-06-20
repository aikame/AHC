from django.shortcuts import render
from rest_framework.response import Response
from rest_framework.views import APIView
from rest_framework import status
from rest_framework.decorators import api_view
from rest_framework.renderers import JSONRenderer
from SCTDB.models import Profile
from SCTDB.serializers import ProfileSerializer,IdSerializer
import json
import requests
# Create your views here.

@api_view(['POST','GET'])
def profile_detail(request):
    print(request.data)
    try:
        profile = Profile.objects
    except Profile.DoesNotExist:
        return Response(status=status.HTTP_404_NOT_FOUND)
    
    if request.method == 'POST':
        serializer = ProfileSerializer(data=request.data)
        print(serializer.is_valid())
        if serializer.is_valid():
            content = JSONRenderer().render(serializer.data)
            response = requests.post('http://localhost:9200/users/_doc',data=content,headers={"Content-Type":"application/json"})
            return Response(response.content)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
    
    elif request.method == 'GET':
        response = requests.get('http://localhost:9200/users/_search')
        return Response(response.json())



@api_view(['GET','DELETE', 'POST'])
def get_one(request):
    if request.method == 'GET':
        data = json.loads(request.body)
        response = requests.get("http://localhost:9200/users/_search", data='{"query": {"term": {"_id": "'+ data["id"]+'"}}}',headers={"Content-Type":"application/json"})
        return Response(response.json())
    elif request.method == 'DELETE':
        data = json.loads(request.body)
        response = requests.delete(f"http://localhost:9200/users/_doc/{data['id']}",headers={"Content-Type":"application/json"})
        return Response(response.json())
    elif request.method == 'POST':
        print(request.body)
        return Response(data="{'status':'success'}", status=status.HTTP_201_CREATED)
@api_view(['GET'])
def get_text(request):
    data = json.loads(request.body)
    data_text = '{"query": {"simple_query_string": {"query": "'+ data["text"] +'"}}}'
    response = requests.get("http://localhost:9200/users/_search", data=data_text.encode('utf-8'),headers={"Content-Type":"application/json"})
    return Response(response.json())
