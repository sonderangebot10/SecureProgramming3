import React, { Component } from 'react';

export class FetchData extends Component {
  constructor(props) {
    super(props);
    this.state = { threads: 0, loading: true };
    this.AddThread = this.AddThread.bind(this);
    this.RemoveThread = this.RemoveThread.bind(this);
  }

  componentDidMount() {

  }

  render() {
    return (
      <div>
        <h1 id="tabelLabel" >Multi Threaded File Reader</h1>
        <p>Threads: {this.state.threads}</p>

        <button onClick={this.AddThread}>Add</button>
        <button onClick={this.RemoveThread}>Remove</button>
      </div>
    );
  }

  async AddThread () {
    const response = await fetch('read/addthread');
      const data = await response.json();
      this.setState({ threads: data, loading: false }); 
    }

  async RemoveThread() {
        const response = await fetch('read/removethread');
        const data = await response.json();
        this.setState({ threads: data, loading: false });
    }
}
