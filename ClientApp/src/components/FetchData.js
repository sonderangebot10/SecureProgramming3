import React, { Component } from 'react';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { Line } from 'rc-progress';

export class FetchData extends Component {
  constructor(props) {
    super(props);
        this.state = { threads: 0, loading: true, maxPrime: 0, minPrime: 0, totalFilesDone: 0, filesStatus: [] };
        this.AddThread = this.AddThread.bind(this);
        this.RemoveThread = this.RemoveThread.bind(this);
    }

    componentDidMount() {
        const newConnection = new HubConnectionBuilder()
            .withUrl('/primeHub')
            .withAutomaticReconnect()
            .build();

        if (newConnection) {
            newConnection.start()
                .then(result => {
                    console.log('Connected!');

                    newConnection.on('ChangeMax', value => {
                        this.setState({ maxPrime: value });
                    });

                    newConnection.on('ChangeMin', value => {
                        this.setState({ minPrime: value });
                    });

                    newConnection.on('ChangeTotalFilesDone', value => {
                        this.setState({ totalFilesDone: value });
                    }); 

                    newConnection.on('ChangeFile', (fileName, total, current) => {
                        var dict = this.state.filesStatus;
                        dict[fileName] = { total: total, current: current };

                        this.setState({ filesStatus: dict });
                    });
                })
                .catch(e => console.log('Connection failed: ', e));
        }

        this.GetThreads();
  }

    render() {
        const loading = this.state.loading;
        const fileData = this.state.filesStatus;

        let files = [];

        for (var key in fileData) {
            if (fileData.hasOwnProperty(key)) {
                if (fileData[key].current > 0)
                    files.push([key, fileData[key].total, fileData[key].current]);
            }
        }

        return (
          <div>
            <h1 id="tabelLabel" >Multi Threaded File Reader</h1>
            <p>Threads: {this.state.threads}</p>

            <button onClick={this.AddThread} disabled={loading}>Add</button>
            <button onClick={this.RemoveThread} disabled={loading}>Remove</button>
            {loading ? <p>Waiting for thread nr {this.state.threads} to finish...</p> : <p>----------</p>}
            <p>Max prime: {this.state.maxPrime}</p>
            <p>Min prime: {this.state.minPrime}</p>  
            <p>Total files done: {this.state.totalFilesDone}</p>
                <ul>
                    {files.map(values => {
                        let [key, total, current] = values;
                        console.log(parseInt((total - current) / total * 100))
                        console.log(parseInt((total - current) / total * 100).toString());
                        return <li key={key}>{key} <Line percent={parseInt((total - current) / total * 100).toString()} strokeWidth="4" strokeColor="#D3D3D3" /></li>
                    })}
                </ul>
          </div>
        );
  }

    async AddThread() {
        this.setState({ loading: true });

        await fetch('read/addthread')
            .then(response => response.json())
            .then(data => {
                this.setState({ threads: data, loading: false });
            });
    }

    async RemoveThread() {
        this.setState({ loading: true });

        await fetch('read/removethread')
            .then(response => response.json())
            .then(data => {
                this.setState({ threads: data, loading: false });
            });
    }

    async GetThreads() {
        await fetch('read/getThreads')
            .then(response => response.json())
            .then(data => {
                this.setState({ threads: data, loading: false });
            });
    }
}
