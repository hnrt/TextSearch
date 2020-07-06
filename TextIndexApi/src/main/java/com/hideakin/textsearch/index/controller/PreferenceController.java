package com.hideakin.textsearch.index.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RestController;

import com.hideakin.textsearch.index.model.PreferenceRequest;
import com.hideakin.textsearch.index.model.ValueResponse;
import com.hideakin.textsearch.index.service.PreferenceService;

@RestController
public class PreferenceController {

	@Autowired
	private PreferenceService preferenceService;
	
	@RequestMapping(value="/v1/preferences/{name}",method=RequestMethod.GET)
	public ResponseEntity<?> getPreference(
			@PathVariable String name) {
		String value = preferenceService.getPreference(name);
		if (value != null) {
			return new ResponseEntity<>(new ValueResponse(value), HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

	@RequestMapping(value="/v1/preferences",method=RequestMethod.POST)
	public ResponseEntity<?> createPreference(
			@RequestBody PreferenceRequest req) {
		return new ResponseEntity<>(preferenceService.setPreference(req.getName(), req.getValue()) ? HttpStatus.CREATED : HttpStatus.OK);
	}

	@RequestMapping(value="/v1/preferences/{name}",method=RequestMethod.PUT)
	public ResponseEntity<?> updatePreference(
			@PathVariable String name,
			@RequestBody PreferenceRequest req) {
		return new ResponseEntity<>(preferenceService.resetPreference(name, req.getName(), req.getValue()) ? HttpStatus.OK : HttpStatus.NOT_FOUND);
	}

	@RequestMapping(value="/v1/preferences/{name}",method=RequestMethod.DELETE)
	public ResponseEntity<?> deletePreference(
			@PathVariable String name) {
		return new ResponseEntity<>(preferenceService.deletePreference(name) ? HttpStatus.OK : HttpStatus.NOT_FOUND);
	}

}
