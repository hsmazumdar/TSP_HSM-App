"""Score step-1 tours: true Euclidean length from the .xy coordinates, gap to the best long-LKH tour."""
import numpy as np, pandas as pd, os, sys
D=sys.argv[1]; X=sys.argv[2]
def read_tour(path):
    if path.endswith('.tour'):
        L=open(path).read().split('TOUR_SECTION')[1].split(); out=[]
        for v in L:
            v=int(v)
            if v==-1: break
            out.append(v-1)
        return np.array(out)
    if path.endswith('.route'):
        return np.array([int(l.split(',')[0]) for l in open(path).read().split('\n')[1:] if l.strip()])
    return np.array([int(l) for l in open(path).read().split() if l.strip()])
r=pd.read_csv(os.path.join(D,'results.csv')); xyc={}; L=[]
for _,row in r.iterrows():
    if row.instance not in xyc: xyc[row.instance]=np.loadtxt(os.path.join(X,row.instance+'.xy'),skiprows=1)
    p=xyc[row.instance]; f=os.path.join(D,'tours',str(row.tour_file))
    try:
        t=read_tour(f); ok=len(t)==len(p) and len(set(t))==len(p)
        L.append(np.hypot(*(p[t]-p[np.roll(t,-1)]).T).sum() if ok else np.nan)
    except Exception as e: L.append(np.nan)
r['length']=L
print(r.to_string())
r.to_csv(os.path.join(D,'results_scored.csv'),index=False)
