#!/usr/bin/env python3
# -*- coding: utf-8 -*-
# author： ShanK
# datetime： 2021/5/9 15:23 
# ide： PyCharm
import time
import socket

def connect():
    udp_server = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    udp_addr = ("192.168.3.23", 10086)
    udp_server.bind(udp_addr)
    udp_server.settimeout(10)

    msg, client = udp_server.recvfrom(1024)
    msg = msg.decode()
    msg = msg.split("+")
    return msg
