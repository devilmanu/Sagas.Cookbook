import { v4 as uuidv4 } from 'uuid';
import React, { Component } from 'react';
import { webTracerWithZone  } from '../tracer';
import { context, trace } from '@opentelemetry/api';

export class FetchData extends Component {
    static displayName = FetchData.name;

  constructor(props) {
    super(props);
    this.state = { forecasts: [], loading: true };

  }

    componentDidMount() {
        this.populateOrders();
  }

  static renderForecastsTable(forecasts) {
      return (
     <div>
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
            <div>
                <button className="btn btn-primary" onClick={this.PostOrder}>Create</button>
            </div>
        <p>This component demonstrates fetching data from the server.</p>
        {contents}
      </div>
    );
    }



    addIconsStatus(status) {
        if (status === 'Submited') { return ` ${status}`}
        if (status === 'Accepted') { return ` ${status}` }
        if (status === 'Shiped') { return ` ${status}` }
        if (status === 'Finished') { return ` ${status}` }
    }

  async populateOrders() {
    const response = await fetch('https://localhost:7134/api/Orders');
    const data = await response.json();
    this.setState({ forecasts: data, loading: false });
    }

    PostOrder() {
        const span = webTracerWithZone.startSpan('post-order');

        let data = {
            id: uuidv4(),
            pizzas: ["Pruebas"],
            createdAt: new Date().toISOString(),
            userId: "Front",
            userName: "Pruebas Front",
            throwError : false
        }
        context.with(trace.setSpan(context.active(), span), () => {
            fetch('https://localhost:7134/api/Orders', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(data)
            }).then((data) => {
                span.end();
                return data;
            })
        });
        
    }

    async DeleteOrders() {

        const response = await fetch('https://localhost:7134/api/Orders', {
            method: 'DELETE',
        });
        const dataStatus = await response;

    }
}
