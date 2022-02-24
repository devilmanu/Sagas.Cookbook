import React, { Component } from 'react';
import { v4 as uuidv4 } from 'uuid';

export class Counter extends Component {
  static displayName = Counter.name;

  constructor(props) {
    super(props);
    this.state = { currentCount: 0 };
    this.incrementCounter = this.incrementCounter.bind(this);
  }

  incrementCounter() {
    this.setState({
      currentCount: this.state.currentCount + 1
    });
  }

  render() {
    return (
      <div>
        <h1>Counter</h1>

        <p>This is a simple example of a React component.</p>

        <p aria-live="polite">Current count: <strong>{this.state.currentCount}</strong></p>

            <button className="btn btn-primary" onClick={this.PostOrder}>Populate</button>

            <button className="btn btn-primary" onClick={this.incrementCounter}>Increment</button>
      </div>
    );
    }


    async PostOrder() {
        let data = {
            id: uuidv4(),
            pizzas: ["Pruebas"],
            createdAt: new Date().toISOString(),
            userId: "Front",
            userName: "Pruebas Front",
            "throwError": false
        }

        const response = await fetch('https://localhost:7134/api/Orders', {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });
        const dataStatus = await response;
    }
}
