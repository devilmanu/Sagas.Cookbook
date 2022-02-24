import React, { Component } from 'react';
import { v4 as uuidv4 } from 'uuid';

export class FetchData extends Component {
  static displayName = FetchData.name;

  constructor(props) {
    super(props);
    this.state = { forecasts: [], loading: true };
  }

  componentDidMount() {
    this.populateWeatherData();
  }

  static renderForecastsTable(forecasts) {
      return (
     <div>

          <button className="btn btn-primary" onClick={this.PostOrder}>Create Order</button>
          <table className='table table-striped' aria-labelledby="tabelLabel">
            <thead>
              <tr>
                <th>Id</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {forecasts.map(forecast =>
                  <tr key={forecast.id}>
                      <td>{forecast.id}</td>
                      <td>{forecast.status}</td>
                </tr>
              )}
            </tbody>
          </table>
      </div>
    );
  }

  render() {
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : FetchData.renderForecastsTable(this.state.forecasts);

    return (
      <div>
        <h1 id="tabelLabel" >Weather forecast</h1>
        <p>This component demonstrates fetching data from the server.</p>
        {contents}
      </div>
    );
  }

  async populateWeatherData() {
    const response = await fetch('https://localhost:7134/api/Orders');
    const data = await response.json();
    this.setState({ forecasts: data, loading: false });
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
