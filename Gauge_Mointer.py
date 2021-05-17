import RPi.GPIO as GPIO
import tkinter as tk
import pyvisa
import socket
import time
import sys
import re

limit1 = 1e-7
limit2 = 1e-5
limit3 = 1e-5
limit4 = 1e-5

host = '192.168.2.11'
p1 = 10004
p2 = 10003
p3 = 10001
p4 = 10002

#instantiate
GPIO.setmode(GPIO.BCM)
GPIO.setup(26,GPIO.IN,pull_up_down=GPIO.PUD_DOWN)
rm = pyvisa.ResourceManager()
usb_inst=rm.open_resource('USB0::10893::20481::MY58002023::0::INSTR')
s1 = socket.socket()
s1.connect((host,p1))
s2 = socket.socket()
s2.connect((host,p2))
s3 = socket.socket()
s3.connect((host,p3))
s4 = socket.socket()
s4.connect((host,p4))
udp_client = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
change = 0
counts = 0

def rec_pgc(socket):
    socket.send(b'RPV3\r')
    strb = socket.recv(1024).decode()
    stra = re.findall(r'\d+\.?\d*[eE][-+ ]\d+',strb)
    if len(stra)==0:
        p = 0
    else:
        p = float(stra[0])
    return p

def rec_wrg(socket):
    socket.send(b'?V913\r')
    strb = socket.recv(1024).decode()
    stra = re.findall(r'\d+\.?\d*[eE][-+ ]\d+',strb)
    if len(stra)==0:
        p = 0
    else:
        p = float(stra[0])
    return (p*0.0075006)

def on_closing():
    window.destroy()
    usb_inst.write('SYST:LOC')
    usb_inst.close()
    udp_client.close()
    s1.close()
    s2.close()
    s3.close()
    s4.close()
    sys.exit(0)

window = tk.Tk()
window.title('Gauge_Monitor')
w,h = window.maxsize()
window.geometry("{}x{}".format(w, h))
gpios = tk.StringVar()
text1 = tk.StringVar()
text2 = tk.StringVar()
text3 = tk.StringVar()
text4 = tk.StringVar()
l = tk.Label(window, textvariable=gpios,bg='yellow',fg='red',font=('Arial',30),width=30,heigh=1)
l.pack()
l1 = tk.Label(window, textvariable=text1,bg='green',fg='white',font=('Arial',64),width=30,heigh=1)
l1.pack()
l2 = tk.Label(window, textvariable=text2,bg='green',fg='white',font=('Arial',64),width=30,heigh=2)
l2.pack()
l3 = tk.Label(window, textvariable=text3,bg='green',fg='white',font=('Arial',64),width=30,heigh=2)
l3.pack()
l4 = tk.Label(window, textvariable=text4,bg='green',fg='white',font=('Arial',64),width=30,heigh=2)
l4.pack()
window.protocol("WM_DELETE_WINDOW",on_closing)

while True:
    v1 = rec_pgc(s1)
    v2 = rec_wrg(s2)
    v3 = rec_pgc(s3)
    v4 = rec_pgc(s4)
    msg = b"%.2e+%.2e+%.2e+%.2e"%(v1,v2,v3,v4)
    udp_add = ("192.168.3.23",10086)
    udp_client.sendto(msg, udp_add)
    text1.set("Ion Trap: \t%.2e Torr"%v1)
    text2.set("Beam 1: \t%.2e Torr"%v2)
    text3.set("Beam 2: \t%.2e Torr"%v3)
    text4.set("REMPI: \t%.2e Torr"%v4)
    if GPIO.input(26) == 0:
        gpios.set('Alarm: False .....'[0:13+counts%6])
        if change == 1:
            usb_inst.write('CALC:LIM:UPP 30,(@101)')
            usb_inst.write('SYST:LOC')
            change = 0
        elif ((v1>limit1)|(v2>limit2)|(v3>limit3)|(v4>limit4)):
            gpios.set('Warning: Air Leakage!!!')
            change = 0
    else:
        gpios.set('Alarm: True .....'[0:12+counts%6])
        if change == 0 and ((v1>limit1)|(v2>limit2)|(v3>limit3)|(v4>limit4)):
            usb_inst.write('CALC:LIM:UPP 0,(@101)')
            gpios.set('Beep: Air Leakage!!!')
            change = 1
        elif ((v1>limit1)|(v2>limit2)|(v3>limit3)|(v4>limit4)):
            gpios.set('Beep: Air Leakage!!!')
            change = 1
        else:
            change = 0
    window.update()
    counts+=1
    if counts==65535:
        counts=0
    time.sleep(0.5)