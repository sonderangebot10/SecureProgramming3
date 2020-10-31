import React, { Component } from 'react';

export class FetchData extends Component {
  constructor(props) {
    super(props);
    this.state = { threads: 0, loading: false };
    this.AddThread = this.AddThread.bind(this);
    this.RemoveThread = this.RemoveThread.bind(this);
  }

  componentDidMount() {

  }

    render() {
        const loading = this.state.loading;

    return (
      <div>
        <h1 id="tabelLabel" >Multi Threaded File Reader</h1>
        <p>Threads: {this.state.threads}</p>

        <button onClick={this.AddThread} disabled={loading}>Add</button>
        <button onClick={this.RemoveThread} disabled={loading}>Remove</button>
        {loading && <p>Waiting for thread nr { this.state.threads } to finish...</p> }    
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
}
