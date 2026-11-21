"""Score step-2 tours with the official TSPLIB metric (EUC_2D = nint, CEIL_2D = ceil) on the ORIGINAL coordinates."""
import numpy as np, pandas as pd, os, sys
D=sys.argv[1]; T=sys.argv[2]   # D = step2 folder with results.csv + tours, T = folder with original .tsp files
opt=pd.read_csv(os.path.join(os.path.dirname(os.path.abspath(__file__)),'optima.csv')).set_index('instance')
def coords(n):
    txt=open(os.path.join(T,n+'.tsp')).read()
    return np.array(txt.split('NODE_COORD_SECTION')[1].split('EOF')[0].split(),dtype=float).reshape(-1,3)[:,1:]
def read_tour(path):
    if path.endswith('.tour'):
        out=[]
        for v in open(path).read().split('TOUR_SECTION')[1].split():
            v=int(v)
            if v==-1: break
            out.append(v-1)
        return np.array(out)
    if path.endswith('.route'):
        return np.array([int(l.split(',')[0]) for l in open(path).read().split('\n')[1:] if l.strip()])
    return np.array([int(l) for l in open(path).read().split() if l.strip()])
r=pd.read_csv(os.path.join(D,'results.csv')); C={}; L=[]
for _,row in r.iterrows():
    n=row.instance
    if n not in C: C[n]=coords(n)
    p=C[n]
    try:
        t=read_tour(os.path.join(D,'tours',str(row.tour_file)))
        if len(t)!=len(p) or len(set(t))!=len(p): L.append(np.nan); continue
        d=np.hypot(*(p[t]-p[np.roll(t,-1)]).T)
        d=np.ceil(d) if opt.loc[n,'edge_weight']=='CEIL_2D' else np.floor(d+0.5)
        L.append(d.sum())
    except Exception: L.append(np.nan)
r['length']=L; r['optimum']=[opt.loc[n,'optimum'] for n in r.instance]; r['gap_pct']=100*(r.length/r.optimum-1)
r.to_csv(os.path.join(D,'results_scored.csv'),index=False)
s=r.groupby(['instance','method']).agg(N=('instance',lambda x: opt.loc[x.iloc[0],'N']),gap_mean=('gap_pct','mean'),gap_sd=('gap_pct','std'),wall=('wall_s','mean'),peak=('peak_mb','mean'),runs=('length','count')).reset_index()
print(s.sort_values(['N','method']).to_string(index=False))
